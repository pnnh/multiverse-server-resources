import commonjs from '@rollup/plugin-commonjs'
import { nodeResolve } from '@rollup/plugin-node-resolve'
import { visualizer } from 'rollup-plugin-visualizer'
import strip from '@rollup/plugin-strip'
import esbuild from 'rollup-plugin-esbuild'
import alias from '@rollup/plugin-alias'
import scss from 'rollup-plugin-scss'
import path from 'path'
import replace from '@rollup/plugin-replace'
import del from 'rollup-plugin-delete'

const customResolver = nodeResolve({
  extensions: ['.mjs', '.js', '.jsx', '.json', '.sass', '.scss']
})

const isProduction = process.env.NODE_ENV === 'production';

const outDir = '../Gliese/wwwroot/assets'

export default {
  input: 'src/index.tsx',
  preserveEntrySignatures: false,
  output: {
    dir: outDir, 
    format: 'esm'
  },
  plugins: [
    del({ targets: outDir + '/*', force: true }),
    alias({
      entries: [
        { find: '@', replacement: path.resolve(__dirname, 'src') },
        //{ find: 'batman-1.0.0', replacement: './joker-1.5.0' }
      ],
      customResolver
    }),
    commonjs(),
    nodeResolve(),
    esbuild({
      // All options are optional
      include: /\.[jt]sx?$/, // default, inferred from `loaders` option
      exclude: /node_modules/, // default
      sourceMap: true, // default
      minify: process.env.NODE_ENV === 'production',
      target: 'esnext', // default, or 'es20XX', 'esnext'
      jsx: 'transform', // default, or 'preserve'
      jsxFactory: 'React.createElement',
      jsxFragment: 'React.Fragment',
      // Like @rollup/plugin-replace
      define: {
        __VERSION__: '"x.y.z"',
      },
      tsconfig: 'tsconfig.json', // default
      // Add extra loaders
      loaders: {
        // Add .json files support
        // require @rollup/plugin-commonjs
        '.json': 'json',
        // Enable JSX in .js files too
        '.js': 'jsx',
      },
    }),
    scss({
      output: outDir + '/index.css',
      watch: ['src']
    }),
    replace({
      'process.env.NODE_ENV': JSON.stringify('production'),
      __buildDate__: () => JSON.stringify(new Date()),
      __buildVersion: 15,
      preventAssignment: true
    }),
    strip({
      include: '**/*.(js|mjs|ts|tsx)',
      debugger: !isProduction,
      functions: isProduction ? ['console.log', 'console.debug'] : [],
      sourceMap: isProduction
    }),
    visualizer({
      filename: 'debug/status.html'
    })
  ]
}
