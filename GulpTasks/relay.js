'use strict';

var gulp = require('gulp'),
    chalk = require('chalk'),
    del = require('del'),
    runSequence = require('run-sequence'),
    zip = require('gulp-zip'),
    rename = require('gulp-rename');

var buildConfig = require('../gulp.config');

gulp.task('relay:deploy', function (done) {
    console.log('Don\'t forget to ' + chalk.yellow('build') + ' Relay with Visual Studio.');

    return runSequence(
        'relay:clear',
        'relay:copy',
        'relay:managementweb',
        'relay:package',
        done
    );
});

gulp.task('relay:clear', function () {
    return del(buildConfig.relay.outputPaths.deploy);
});

gulp.task('relay:managementweb', function () {
    return gulp.src(buildConfig.managementWeb.inputPaths.deploymentFiles)
        .pipe(gulp.dest(buildConfig.relay.outputPaths.managementWeb));
});

gulp.task('relay:copy', function () {
    return gulp.src(buildConfig.relay.inputPaths.debug)
        .pipe(rename(renameConfigFile))
        .pipe(gulp.dest(buildConfig.relay.outputPaths.deploy))
});

gulp.task('relay:package', function () {
    return gulp.src(buildConfig.relay.inputPaths.packageFiles)
        .pipe(zip(buildConfig.relay.outputPaths.packageFileName))
        .pipe(gulp.dest(buildConfig.relay.outputPaths.deploy))
});

function renameConfigFile(p) {
    var fileName = p.basename + p.extname;
    if (fileName === buildConfig.relay.inputPaths.configFileName) {
        p.basename = buildConfig.relay.outputPaths.configFileName;
        p.extname = '';
    }
}
