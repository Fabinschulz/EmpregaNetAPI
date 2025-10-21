<!-- name: CI/CD - EmpregaNet (EB)

on:
  push:
    branches:
      - main
  workflow_dispatch:

env:
  API_PROJECT_NAME: EmpregaNet.Api
  TESTS_PROJECT_PATH: src/EmpregaNet.Tests/EmpregaNet.Tests.csproj
  SOLUTION_FILE: EmpregaNet.sln
  
  EB_APPLICATION_NAME: empreganet-api-app
  EB_ENVIRONMENT_NAME: empreganet-api-prod-env
  REGION: us-west-2
  DEPLOYMENT_PACKAGE_PATH: ./publish_output
  ZIP_FILE_NAME: deploy_package.zip

jobs:
  build: 
    name: Build e Publicação
    runs-on: ubuntu-latest
    
    steps:
      - name: Checkout Code
        uses: actions/checkout@v4

      - name: Setup .NET 9 SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.x

      - name: Restore dependencies
        run: dotnet restore ${{ env.SOLUTION_FILE }}

      - name: Build Solution
        run: dotnet build ${{ env.SOLUTION_FILE }} --configuration Release --no-restore
        
      - name: Publish API for Deployment
        run: dotnet publish src/${{ env.API_PROJECT_NAME }} --configuration Release --output ${{ env.DEPLOYMENT_PACKAGE_PATH }} --no-build

      - name: Create Deployment Zip
        run: |
          cd ${{ env.DEPLOYMENT_PACKAGE_PATH }}
          zip -r ../${{ env.ZIP_FILE_NAME }} .

      - name: Archive Deployment Artifact
        uses: actions/upload-artifact@v4
        with:
          name: eb-deployment-package
          path: ${{ env.ZIP_FILE_NAME }}
          retention-days: 1 
          
  test:
    name: Testes Unitários e de Integração
    runs-on: ubuntu-latest
    needs: build
    
    steps:
      - name: Checkout Code
        uses: actions/checkout@v4

      - name: Setup .NET 9 SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 9.x
          
      - name: Run Tests
        run: dotnet test ${{ env.TESTS_PROJECT_PATH }} --configuration Release --verbosity normal

  deploy:
    name: Deploy para Elastic Beanstalk
    needs: [build, test] 
    runs-on: ubuntu-latest
    
    if: github.ref == 'refs/heads/main' && github.event_name == 'push'

    steps:
      - name: Configure AWS Credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: ${{ env.REGION }}
          
      - name: Download Deployment Artifact
        uses: actions/download-artifact@v4
        with:
          name: eb-deployment-package
          path: . 

      - name: Deploy Application Version to Elastic Beanstalk
        uses: aws-actions/elasticbeanstalk-deploy@v1
        with:
          application_name: ${{ env.EB_APPLICATION_NAME }}
          environment_name: ${{ env.EB_ENVIRONMENT_NAME }}
          version_label: empreganet-api-${{ github.sha }}-latest 
          deployment_package: ${{ env.ZIP_FILE_NAME }} -->
