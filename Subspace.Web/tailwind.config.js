module.exports = {
    darkMode: 'class',
    content: [
        './Views/**/*.cshtml',
        './Areas/**/*.cshtml',
        './wwwroot/js/**/*.js'
    ],
    safelist: [],
    theme: {
        extend: {
            keyframes: {
                shimmer: {
                    '0%': { backgroundPosition: '-1000px 0' },
                    '100%': { backgroundPosition: '1000px 0' },
                },
            },
            animation: {
                'loading-shimmer': 'shimmer 2s infinite linear',
            },
        }
    },
    plugins: []
}