'use strict';

var path = require('path');

// All paths are relative from the repository root!

module.exports = {
    docs: {
        inputPath: 'Documentation',
        outputPaths: {
            website: path.join('Documentations', 'Web'),
            pdf: path.join('Documentations', 'PDF')
        }
    },
    managementWeb: {
        inputPaths: {
            basePath: 'Thinktecture.Relay.ManagementWeb',
            app: {
                index: path.join('Thinktecture.Relay.ManagementWeb', 'index.html'),
                less: path.join('Thinktecture.Relay.ManagementWeb', 'assets', 'less', '*.less'),
                js: [
                    path.join('Thinktecture.Relay.ManagementWeb', 'app', 'appInit.js'),
                    path.join('Thinktecture.Relay.ManagementWeb', 'app', '**', '*.js'),
                    path.join('Thinktecture.Relay.ManagementWeb', 'appServices', '**', '*.js'),
                    path.join('Thinktecture.Relay.ManagementWeb', 'appTranslations', '**', '*.js')
                ],
                css: [
                    path.join('Thinktecture.Relay.ManagementWeb', 'assets', '*.css')
                ]
            },
            vendor: {
                js: [
                    path.join('Thinktecture.Relay.ManagementWeb', 'libs', 'jQuery', '**', '*.js'),
                    path.join('Thinktecture.Relay.ManagementWeb', 'libs', 'angular', '**', '*.js'),
                    path.join('Thinktecture.Relay.ManagementWeb', 'libs', 'chartjs', '**', '*.js'),
                    path.join('Thinktecture.Relay.ManagementWeb', 'libs', '**', '*.js')
                ],
                css: [

                    path.join('Thinktecture.Relay.ManagementWeb', 'libs', 'bootstrap', '**', '*.css'),
                    path.join('Thinktecture.Relay.ManagementWeb', 'libs', '**', '*.css')
                ]
            },
            deploymentFiles: [
                path.join('Thinktecture.Relay.ManagementWeb', '**', '*.*'),
                '!' + path.join('Thinktecture.Relay.ManagementWeb', 'assets', 'less', '*.*'),
                '!' + path.join('Thinktecture.Relay.ManagementWeb', 'deploy', '**', '*.*')
            ]
        },
        outputPaths: {
            less: path.join('Thinktecture.Relay.ManagementWeb', 'assets'),
            index: path.join('Thinktecture.Relay.ManagementWeb'),
            deploy: path.join('Thinktecture.Relay.ManagementWeb', 'deploy'),
            packageFileName: 'managementWeb.zip'
        }
    },
    relay: {
        inputPaths: {
            debug: [
                path.join('Thinktecture.Relay.Server', 'bin', 'Debug', '*.*')
            ],
            configFileName: 'Thinktecture.Relay.Server.exe.config',
            packageFiles: [
                path.join('deploy', '**', '*.*')
            ]
        },
        outputPaths: {
            configFileName: 'SAMPLE_Thinktecture.Relay.Server.exe.config',
            deploy: 'deploy',
            managementWeb: path.join('deploy', 'ManagementWeb'),
            packageFileName: 'relay.zip'
        }
    },
    release: {
        outputPaths: {
            folder: 'release'
        }
    }
};
