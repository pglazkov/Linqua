var path = require('path');
var webpack = require('webpack');
var ExtractTextPlugin = require('extract-text-webpack-plugin');
var extractCSS = new ExtractTextPlugin('vendor.css');
var isDevBuild = process.argv.indexOf('--env.prod') < 0;

module.exports = {
    resolve: {
        extensions: ['.js', '.scss']
    },
    module: {
        rules: [
            { test: /\.(png|woff|woff2|eot|ttf|svg)$/, loader: 'url-loader?limit=100000' },
            { test: /\.scss$/i, loaders: extractCSS.extract(['css-loader?minimize', 'sass-loader']) },
            { test: /\.json$/, loader: 'json-loader' }
        ]
    },
    entry: {
        // polyfills: [
        //     'core-js/es6/symbol',
        //     'core-js/es6/object',
        //     'core-js/es6/function',
        //     'core-js/es6/parse-int',
        //     'core-js/es6/parse-float',
        //     'core-js/es6/number',
        //     'core-js/es6/math',
        //     'core-js/es6/string',
        //     'core-js/es6/date',
        //     'core-js/es6/array',
        //     'core-js/es6/regexp',
        //     'core-js/es6/map',
        //     'core-js/es6/set',
        //     'core-js/es6/weak-map',
        //     'core-js/es6/weak-set',
        //     'core-js/es6/typed',
        //     'core-js/es6/reflect',
        //     'core-js/es7/reflect',
        //     'zone.js/dist/zone'
        // ],
        vendor: [
            'font-awesome/scss/font-awesome.scss',
            'bootstrap/scss/bootstrap.scss',
            '@angular/common',
            '@angular/compiler',
            '@angular/core',
            '@angular/http',
            '@angular/forms',
            '@angular/platform-browser',
            '@angular/platform-browser-dynamic',
            '@angular/router',
            'rxjs',
            'bootstrap',            
            'ng2-bootstrap',
            'zone.js',
            'reflect-metadata',
            'angular2-jwt'
        ]
    },
    output: {
        path: path.join(__dirname, 'wwwroot', 'dist'),
        filename: '[name].js',
        library: '[name]_[hash]',
    },
    plugins: [
        extractCSS,
        // To eliminate warning
        // https://github.com/AngularClass/angular2-webpack-starter/issues/993
        new webpack.ContextReplacementPlugin(
            /angular(\\|\/)core(\\|\/)(esm(\\|\/)src|src)(\\|\/)linker/,
            __dirname
        ),
        new webpack.DllPlugin({
            path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
            name: '[name]_[hash]'
        })
    ].concat(isDevBuild ? [] : [
        new webpack.optimize.UglifyJsPlugin({
            beautify: false,
            comments: false
        })
    ])
};
