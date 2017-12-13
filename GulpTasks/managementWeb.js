'use strict';

var gulp = require('gulp'),
    less = require('gulp-less'),
    inject = require('gulp-inject'),
    runSequence = require('run-sequence'),
    zip = require('gulp-zip'),
    server = require('gulp-server-livereload');

var buildConfig = require('../gulp.config');

gulp.task('managementweb:less', function () {
    return gulp.src(buildConfig.managementWeb.inputPaths.app.less)
        .pipe(less())
        .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.less));
});

gulp.task('managementweb:inject', function () {
    var injectables = gulp.src([].concat(
        buildConfig.managementWeb.inputPaths.vendor.css,
        buildConfig.managementWeb.inputPaths.app.css,
        buildConfig.managementWeb.inputPaths.vendor.js,
        buildConfig.managementWeb.inputPaths.app.js
    ));

    return gulp.src(buildConfig.managementWeb.inputPaths.app.index)
        .pipe(inject(injectables, {
            ignorePath: buildConfig.managementWeb.inputPaths.basePath,
            addRootSlash: false
        }))
        .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.index));
});

gulp.task('managementweb:start-livereload-server', function () {
    gulp.src(buildConfig.managementWeb.inputPaths.basePath)
        .pipe(server({
            livereload: true
        }));
});

gulp.task('managementweb:live-server', function () {
    runSequence('managementweb:build', 'managementweb:start-livereload-server');
});

gulp.task('managementweb:deploy:package', function () {
    return gulp.src(buildConfig.managementWeb.inputPaths.deploymentFiles)
        .pipe(zip(buildConfig.managementWeb.outputPaths.packageFileName))
        .pipe(gulp.dest(buildConfig.managementWeb.outputPaths.deploy));
});

gulp.task('managementweb:build', function () {
    runSequence(
        'managementweb:less',
        'managementweb:inject'
    );
});

gulp.task('managementweb:deploy', function () {
    runSequence(
        'managementweb:build',
        'managementweb:deploy:package'
    );
});

gulp.task('managementweb:watch', function () {
    runSequence('managementweb:live-server', function () {
        gulp.watch(buildConfig.managementWeb.inputPaths.app.less, ['managementweb:less']);
    });
});
