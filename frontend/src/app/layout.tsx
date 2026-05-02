import type { Metadata } from "next";
import "./globals.scss";

export const metadata: Metadata = {
  title: "EmpregaNet",
  description: "Portal de vagas e recrutamento",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <body>{children}</body>
    </html>
  );
}
