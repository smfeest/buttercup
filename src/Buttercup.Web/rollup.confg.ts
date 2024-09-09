import resolve from '@rollup/plugin-node-resolve';
import terser from '@rollup/plugin-terser';
import typescript from '@rollup/plugin-typescript';
import staticFiles from 'rollup-plugin-static-files';
import { RollupOptions } from 'rollup';

const config: RollupOptions = {
  input: 'scripts/main.ts',
  output: [
    {
      dir: 'wwwroot/assets/scripts',
      sourcemap: true,
      format: 'iife',
    },
    {
      dir: 'wwwroot/prod-assets/scripts',
      entryFileNames: '[name].[hash].js',
      format: 'iife',
      plugins: [terser()],
    },
  ],
  plugins: [resolve(), typescript(), staticFiles({ include: [] })],
};

export default config;
