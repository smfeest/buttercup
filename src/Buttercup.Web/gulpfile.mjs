import * as cheerio from 'cheerio';
import { deleteAsync } from 'del';
import fs from 'fs/promises';
import gulp from 'gulp';
import cleanCss from 'gulp-clean-css';
import less from 'gulp-less';
import rename from 'gulp-rename';
import { pid } from 'node:process';
import webpack from 'webpack';
import webpackStream from 'webpack-stream';

const { dest, parallel, series, src, watch } = gulp;

const paths = {};
paths.scripts = 'scripts';
paths.styles = 'styles';
paths.assets = 'wwwroot/assets';
paths.scriptAssets = `${paths.assets}/scripts`;
paths.styleAssets = `${paths.assets}/styles`;

const buildSprite = async (spiteName, icons) => {
  const symbols = await Promise.all(
    icons.map(async (icon) => {
      const raw = await fs.readFile(
        `node_modules/lucide-static/icons/${icon}.svg`,
        'utf8',
      );
      const $ = cheerio.load(raw, { xmlMode: true });

      const $svg = $('svg');
      const $symbol = $(`<symbol id="icon-${icon}" />`);
      const attributes = $svg.attr();

      for (const name in attributes) {
        if (!['class', 'height', 'width', 'xmlns'].includes(name)) {
          $symbol.attr(name, attributes[name]);
        }
      }

      $symbol.append($svg.children());

      return $.xml($symbol);
    }),
  );

  const sprite = `<svg xmlns="http://www.w3.org/2000/svg">\n${symbols.join('\n')}\n</svg>`;

  const tempFile = `icons/sprites/${spiteName}.${pid}.tmp`;
  await fs.writeFile(tempFile, sprite);
  await fs.rename(tempFile, `icons/sprites/${spiteName}.svg`);
};

const buildIcons = async () => {
  const sprites = JSON.parse(await fs.readFile('icons/sprites.json', 'utf-8'));
  await fs.mkdir('icons/sprites', { recursive: true });
  await Promise.all(
    Object.entries(sprites).map(([name, icons]) => buildSprite(name, icons)),
  );
};

const buildScriptsDev = () => webpackDevScripts().pipe(dest(paths.assets));

const buildScriptsProd = () =>
  webpackScripts({
    mode: 'production',
    output: { filename: 'scripts/[name].prod.js' },
  }).pipe(dest(paths.assets));

const buildScripts = parallel(buildScriptsDev, buildScriptsProd);

const buildStyles = () =>
  src(`${paths.styles}/main.less`)
    .pipe(less({ math: 'parens-division' }))
    .pipe(dest(paths.styleAssets))
    .pipe(rename({ suffix: '.prod' }))
    .pipe(cleanCss())
    .pipe(dest(paths.styleAssets));

const clean = () =>
  deleteAsync([
    'icons/sprites',
    `${paths.scriptAssets}/**/*`,
    `${paths.styleAssets}/**/*`,
  ]);

const watchIconsAfterBuild = () => watch('icons/sprites.json', buildIcons);

const watchIcons = series(buildIcons, watchIconsAfterBuild);

const watchScripts = () =>
  webpackDevScripts({
    watch: true,
    watchOptions: {
      ignored: /node_modules/,
    },
  }).pipe(dest(paths.assets));

const watchStylesAfterBuild = () =>
  watch(`${paths.styles}/**/*.less`, buildStyles);

const watchStyles = series(buildStyles, watchStylesAfterBuild);

const webpackDevScripts = (config) =>
  webpackScripts({
    mode: 'development',
    devtool: 'eval-cheap-module-source-map',
    output: { filename: 'scripts/[name].js' },
    ...config,
  });

const webpackScripts = (config) =>
  src(`${paths.scripts}/main.ts`).pipe(
    webpackStream(
      {
        resolve: {
          extensions: ['.js', '.ts'],
        },
        module: {
          rules: [
            {
              test: /\.ts$/,
              use: 'ts-loader',
              exclude: /node_modules/,
            },
          ],
        },
        ...config,
      },
      webpack,
    ),
  );

const build = parallel(buildIcons, buildScripts, buildStyles);

const rebuild = series(clean, build);

const watchAll = parallel(watchIcons, watchScripts, watchStyles);

export {
  build,
  buildIcons,
  buildScripts,
  buildScriptsDev,
  buildScriptsProd,
  buildStyles,
  clean,
  build as default,
  rebuild,
  watchAll as watch,
  watchIcons,
  watchScripts,
  watchStyles,
};
