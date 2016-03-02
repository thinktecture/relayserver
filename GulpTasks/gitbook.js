'use strict';

var gulp = require('gulp'),
    Q = require('q'),
    gitbook = require('gitbook'),
    chalk = require('chalk');

var buildConfig = require('../gulp.config');

gulp.task('gitbook', function () {
    var book = new gitbook.Book(buildConfig.docs.inputPath, {
        config: {
            output: buildConfig.docs.outputPaths.website
        }
    });

    return Q.all(book.parse())
        .then(function () {
            return book.generate('website');
        });
});

gulp.task('gitbook:pdf', function () {
    var book = new gitbook.Book(buildConfig.docs.inputPath, {
        config: {
            output: buildConfig.docs.outputPaths.pdf
        }
    });

    return Q.all(book.parse())
        .then(function () {
            return book.generate('pdf');
        }, function () {
            console.log(chalk.red('Error') + ' creating a PDF version of the documentation.');
            console.log('Have you installed ' + chalk.yellow('calibri') + ' correctly?');
        });
});
