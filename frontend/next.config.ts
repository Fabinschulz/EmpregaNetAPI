import type { NextConfig } from 'next';

const apiInternalOrigin = process.env.API_INTERNAL_BASE_URL?.trim().replace(/\/+$/, '');

const nextConfig: NextConfig = {
  cacheComponents: true,
  reactStrictMode: true,
  turbopack: {
    root: import.meta.dirname
  },
  async rewrites() {
    if (!apiInternalOrigin) return [];

    return [
      {
        source: '/api/:path*',
        destination: `${apiInternalOrigin}/api/:path*`
      }
    ];
  }
};

export default nextConfig;
