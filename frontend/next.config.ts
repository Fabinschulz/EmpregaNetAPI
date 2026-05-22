import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  cacheComponents: true,
  reactStrictMode: true,
  turbopack: {
    root: import.meta.dirname
  }
};

export default nextConfig;
