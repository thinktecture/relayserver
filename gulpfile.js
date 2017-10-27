/// <binding BeforeBuild='managementweb:build' />
'use strict';

var gulp = require('gulp');

require('./GulpTasks/gitbook');
require('./GulpTasks/managementWeb');
require('./GulpTasks/relay');
require('./GulpTasks/release');

gulp.task('watch', ['managementweb:watch']);
