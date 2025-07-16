import { defineConfig } from 'vite';

export default defineConfig({
  appType: 'custom',
  // server: {
  //   cors: {
  //     // the origin you will be accessing via browser
  //     origin: 'http://my-backend.example.com',
  //   },
  // },
  build: {
    // generate .vite/manifest.json in outDir
    manifest: 'manifest.json',
    outDir: 'wwwroot/prod-assets2',
    assetsDir: '',
    rollupOptions: {
      // overwrite default .html entry
      input: [
        '/scripts/main.js',
        '/styles/main.less',
        '/styles/print.less',
        '/wwwroot/assets/images/icon-16.png',
      ],
    },
  },
});
