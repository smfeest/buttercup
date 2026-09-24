import * as cheerio from 'cheerio';
import cleanCss from 'gulp-clean-css';
import { deleteAsync } from 'del';
import fs from 'fs/promises';
import gulp from 'gulp';
import less from 'gulp-less';
import rename from 'gulp-rename';
import webpack from 'webpack';
import webpackStream from 'webpack-stream';

const { dest, parallel, series, src, watch } = gulp;

const paths = {};
paths.icons = 'icons';
paths.scripts = 'scripts';
paths.styles = 'styles';
paths.assets = 'wwwroot/assets';
paths.scriptAssets = `${paths.assets}/scripts`;
paths.styleAssets = `${paths.assets}/styles`;

const buildIcons = async () => {
  const symbols = await Promise.all(
    ['chef-hat', 'cooking-pot'].map(async (name) => {
      const raw = await fs.readFile(
        `node_modules/lucide-static/icons/${name}.svg`,
        'utf8',
      );
      const $ = cheerio.load(raw, { xmlMode: true });

      const $svg = $('svg');
      const $symbol = $(`<symbol id="icon-${name}" />`);
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

  await fs.mkdir(paths.icons, { recursive: true });
  await fs.writeFile(`${paths.icons}/sprite.svg`, sprite);
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
    `${paths.icons}/**/*`,
    `${paths.scriptAssets}/**/*`,
    `${paths.styleAssets}/**/*`,
  ]);

const watchIcons = () =>
  watch('node_modules/lucide-static/icons/*.svg', buildIcons);

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
  build as default,
  build,
  buildIcons,
  buildScripts,
  buildScriptsDev,
  buildScriptsProd,
  buildStyles,
  clean,
  rebuild,
  watchAll as watch,
  watchIcons,
  watchScripts,
  watchStyles,
};
