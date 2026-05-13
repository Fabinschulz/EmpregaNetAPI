import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  cacheComponents: true,
  reactStrictMode: true,
  // Pin Turbopack root to this app so a lockfile in a parent folder (e.g. %USERPROFILE%)
  // is not chosen over frontend/pnpm-lock.yaml.
  turbopack: {
    root: import.meta.dirname
  }
};

export default nextConfig;
