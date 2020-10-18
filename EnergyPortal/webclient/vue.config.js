module.exports = {
    outputDir: '../wwwroot',
    pages: {
        app: {
            entry: 'src/main.js',
            template: 'public/index.html',
            filename: 'index.html',
            excludeChunks: ['silent-renew-oidc']
        },
        silentrenewoidc: {
            entry: 'src/silent-renew-oidc.js',
            template: 'public/silent-renew-oidc.html',
            filename: 'silent.html',
            excludeChunks: ['app']
        }
    }
}