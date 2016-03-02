'use strict';

var path = require('path');

var gulp = require('gulp'),
    del = require('del'),
    runSequence = require('run-sequence'),
    release = require('gulp-github-release');

var buildConfig = require('../gulp.config');

gulp.task('release', function (done) {
    return runSequence(
        'check-credentials',
        'managementweb:deploy',
        'relay:deploy',
        'upload-to-github',
        done
    );
});

gulp.task('check-credentials', function (done) {
    try {
        var credentials = require('../credentials.json');

        if (!credentials.token) {
            return done('credentials.json found, but no token inside.');
        }

        done();
    }
    catch (e) {
        done('credentials.json not found');
    }
});

gulp.task('upload-to-github', function () {
    var credentials = require('../credentials.json');
    var packageJson = require('../package.json');

    return gulp.src([
        path.join(buildConfig.managementWeb.outputPaths.deploy, buildConfig.managementWeb.outputPaths.packageFileName),
        path.join(buildConfig.relay.outputPaths.deploy, buildConfig.relay.outputPaths.packageFileName)
    ])
        .pipe(release({
            token: credentials.token,
            manifest: packageJson,
            draft: true
        }));
});
