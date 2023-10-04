// See https://khalidabuhakmeh.com/running-vite-with-aspnet-core-web-applications
// This setup is needed to use the same certificates for both ASP.Core as the Vite dev server
// This is necessary to be able to run Portal locally using HTTPS
import {spawn} from "child_process";
import fs from "fs";
import path from "path";
import {UserConfig, defineConfig} from 'vite';
import vue2 from '@vitejs/plugin-vue2'
import {PreRenderedAsset} from "rollup";

// @ts-ignore
import appsettings from "./appsettings.json";
// @ts-ignore
import appsettingsDev from "./appsettings.Development.json";

import * as process from "process";

// Get base folder for certificates.
const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ''
        ? `${process.env.APPDATA}/ASP.NET/https`
        : `${process.env.HOME}/.aspnet/https`;

// Generate the certificate name using the NPM package name
const certificateName = process.env.npm_package_name;

// Define certificate filepath
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
// Define key filepath
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

// Pattern for image files
const imagePattern = /\.(png|jpe?g|gif|svg|webp|avif)$/;

/**
 * Determine the asset file names when building for production.
 * @param info Information about the prerendered assets.
 */
function determineAssetFileName(info: PreRenderedAsset) {
    if (info.name == 'style.css') { // This is the default output
        return 'css/bundle.css';
    } else if (info.name) {
        // If the file is a CSS file, save it to the css folder
        if (info.name.endsWith('.css')) {
            return 'css/[name][extname]';
        }
        // If the file is an image file, save it to the images folder
        if (imagePattern.test(info.name)) {
            return 'images/[name][extname]';
        }

        // If the file is any other type of file, save it to the assets folder 
        return 'assets/[name][extname]';
    } else {
        // If the file name is not specified, save it to the output directory
        return '[name][extname]';
    }
}

// Export Vite configuration
export default defineConfig(async () => {
    // Ensure the certificate and key exist
    if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
        // Wait for the certificate to be generated
        await new Promise<void>((resolve) => {
            spawn('dotnet', [
                'dev-certs',
                'https',
                '--export-path',
                certFilePath,
                '--format',
                'Pem',
                '--no-password',
            ], {stdio: 'inherit',})
                .on('exit', (code: any) => {
                    resolve();
                    if (code) {
                        process.exit(code);
                    }
                });
        });
    }

    // Define Vite configuration
    const config: UserConfig = {
        clearScreen: true,
        appType: 'mpa',
        root: 'frontend',
        publicDir: 'public',
        plugins: [vue2()],
        build: {
            manifest: appsettings.Vite.Manifest,
            emptyOutDir: false,
            outDir: '../wwwroot',
            assetsDir: '',
            cssCodeSplit: false,
            rollupOptions: {
                // remove hashing, but I could add it back in
                output: {
                    // Save entry files to the appropriate folder
                    entryFileNames: 'js/bundle.js',
                    // Save asset files to the appropriate folder
                    assetFileNames: (info) => {
                        return determineAssetFileName(info);
                    },
                }
            },
        },
        server: {
            port: appsettingsDev.Vite.Server.Port,
            strictPort: true,
            https: {
                cert: certFilePath,
                key: keyFilePath
            },
            hmr: {
                host: "localhost",
                clientPort: appsettingsDev.Vite.Server.Port
            }
        }
    }

    return config;
});