import type { UserConfig } from 'vite';

export default {
  build: {
    manifest: true,

    rollupOptions: {
      input: 'scripts/main.ts',
      output: {
        assetFileNames: '[name].[hash]-sf.[extname]',
      },
    },
  },
} satisfies UserConfig;
