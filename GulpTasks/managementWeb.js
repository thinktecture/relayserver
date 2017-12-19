'use strict';

const gulp = require('gulp'),
  del = require('del'),
  less = require('gulp-less'),
  concat = require('gulp-concat'),
  uglify = require('gulp-uglify'),
  cssmin = require('gulp-cssmin'),
  ngAnnotate = require('gulp-ng-annotate'),
  ngTemplateCache = require('gulp-angular-templatecache'),
  inject = require('gulp-inject'),
  runSequence = require('run-sequence'),
  zip = require('gulp-zip'),
  server = require('gulp-server-livereload'),
  path = require('path');

const buildConfig = require('../gulp.config');

// new

gulp.task('managementweb:clean', () => {
  return del(
    [].concat(
      buildConfig.managementWeb.outputPaths.dist,
      buildConfig.managementWeb.outputPaths.temp
    )
  );
});

gulp.task('managementweb:styles:vendor', () => {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.vendor.css)
    .pipe(concat('vendor.min.css'))
    .pipe(cssmin())
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.styles));
});

gulp.task('managementweb:styles:app', () => {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.app.less)
    .pipe(less())
    .pipe(concat('app.min.css'))
    .pipe(cssmin())
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.styles));
});

gulp.task('managementweb:templates', () => {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.app.templates)
    .pipe(
      ngTemplateCache({
        filename: 'templates.js',
        module: 'thinktectureRelayAdminWeb',
      })
    )
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.temp));
});

gulp.task('managementweb:scripts', () => {
  return gulp
    .src(
      [].concat(
        buildConfig.managementWeb.inputPaths.vendor.js,
        buildConfig.managementWeb.inputPaths.app.js,
        buildConfig.managementWeb.outputPaths.temp + '/templates.js'
      )
    )
    .pipe(ngAnnotate())
    .pipe(concat('app.js'))
    .pipe(uglify())
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.dist));
});

gulp.task('managementweb:files', () => {
  runSequence('managementweb:assets', 'managementweb:fonts');
});

gulp.task('managementweb:assets', () => {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.assets)
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.assets));
});

gulp.task('managementweb:live-server', function() {
  runSequence('managementweb:build', 'managementweb:start-livereload-server');
});

gulp.task('managementweb:fonts', () => {
  runSequence(
    'managementweb:fonts:bootstrap',
    'managementweb:fonts:fontawesome',
    'managementweb:fonts:ui-grid'
  );
});

gulp.task('managementweb:fonts:bootstrap', () => {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.vendor.bootstrap)
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.vendor.fonts));
});

gulp.task('managementweb:deploy', function() {
  runSequence('managementweb:build', 'managementweb:deploy:package');
});

gulp.task('managementweb:fonts:fontawesome', () => {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.vendor.fontawesome)
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.vendor.fonts));
});

gulp.task('managementweb:fonts:ui-grid', () => {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.vendor.uiGrid)
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.vendor.uiGrid));
});

gulp.task('managementweb:inject', () => {
  var injectables = gulp.src(
    [].concat(
      path.join(buildConfig.managementWeb.outputPaths.styles, 'vendor.min.css'),
      path.join(buildConfig.managementWeb.outputPaths.styles, 'app.min.css'),
      //buildConfig.managementWeb.inputPaths.vendor.js,
      path.join(buildConfig.managementWeb.outputPaths.dist, 'app.js')
    )
  );

  return gulp
    .src(buildConfig.managementWeb.inputPaths.app.index)
    .pipe(
      inject(injectables, {
        ignorePath: buildConfig.managementWeb.outputPaths.dist,
        addRootSlash: false,
      })
    )
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.dist));
});

gulp.task('managementweb:build', () => {
  runSequence(
    'managementweb:clean',
    'managementweb:styles:vendor',
    'managementweb:styles:app',
    'managementweb:templates',
    'managementweb:scripts',
    'managementweb:inject',
    'managementweb:files'
  );
});

// OLD

gulp.task('managementweb:less', function() {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.app.less)
    .pipe(less())
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.less));
});

/*
gulp.task('managementweb:inject', function() {
  var injectables = gulp.src(
    [].concat(
      buildConfig.managementWeb.inputPaths.vendor.css,
      buildConfig.managementWeb.inputPaths.app.css,
      buildConfig.managementWeb.inputPaths.vendor.js,
      buildConfig.managementWeb.inputPaths.app.js
    )
  );

  return gulp
    .src(buildConfig.managementWeb.inputPaths.app.index)
    .pipe(
      inject(injectables, {
        ignorePath: buildConfig.managementWeb.inputPaths.basePath,
        addRootSlash: false,
      })
    )
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.index));
});
*/

gulp.task('managementweb:start-livereload-server', function() {
  gulp.src(buildConfig.managementWeb.inputPaths.basePath).pipe(
    server({
      livereload: true,
    })
  );
});

gulp.task('managementweb:live-server', function() {
  runSequence(
    'managementweb:less',
    'managementweb:inject',
    'managementweb:start-livereload-server'
  );
});

gulp.task('managementweb:deploy:package', function() {
  return gulp
    .src(buildConfig.managementWeb.inputPaths.deploymentFiles)
    .pipe(zip(buildConfig.managementWeb.outputPaths.packageFileName))
    .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.deploy));
});

gulp.task('managementweb:deploy', function() {
  runSequence(
    'managementweb:less',
    'managementweb:inject',
    'managementweb:deploy:package'
  );
});

gulp.task('managementweb:watch', function() {
  runSequence('managementweb:live-server', function() {
    gulp.watch(buildConfig.managementWeb.inputPaths.app.less, ['managementweb:less']);
  });
});
