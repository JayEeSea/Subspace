// Dark mode
document.addEventListener('DOMContentLoaded', function () {
    const sunIcon = document.getElementById('sunIcon');
    const moonIcon = document.getElementById('moonIcon');
    const darkModeToggle = document.getElementById('darkModeToggle');

    // Remove initial show-sun/show-moon classes
    document.documentElement.classList.remove('show-sun', 'show-moon');

    // Remove 'hidden' from both icons so they can be shown/hidden by opacity
    sunIcon.classList.remove('hidden');
    moonIcon.classList.remove('hidden');

    const isDark = document.documentElement.classList.contains('dark');

    // Set the checkbox state to match the current theme
    darkModeToggle.checked = isDark;

    function updateIcons() {
        if (document.documentElement.classList.contains('dark')) {
            sunIcon.classList.add('opacity-0');
            sunIcon.classList.remove('opacity-100');
            moonIcon.classList.remove('opacity-0');
            moonIcon.classList.add('opacity-100');
        } else {
            sunIcon.classList.remove('opacity-0');
            sunIcon.classList.add('opacity-100');
            moonIcon.classList.add('opacity-0');
            moonIcon.classList.remove('opacity-100');
        }
    }

    function applyDarkMode(isDark) {
        if (isDark) {
            document.documentElement.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('theme', 'light');
        }
        updateIcons();
    }

    if (darkModeToggle) {
        darkModeToggle.addEventListener('change', function () {
            applyDarkMode(this.checked);
        });

        // Set initial state
        updateIcons();
    }
});