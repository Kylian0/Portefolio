window.portfolioTheme = (() => {
    const storageKey = "portfolio-theme";
    const darkTheme = "dark";
    const lightTheme = "light";

    function getPreferredTheme() {
        const storedTheme = localStorage.getItem(storageKey);

        if (storedTheme === darkTheme || storedTheme === lightTheme) {
            return storedTheme;
        }

        return window.matchMedia("(prefers-color-scheme: dark)").matches
            ? darkTheme
            : lightTheme;
    }

    function apply(theme) {
        document.documentElement.dataset.theme = theme;
        document.documentElement.style.colorScheme = theme;
    }

    return {
        initialize() {
            apply(getPreferredTheme());
        },
        isDark() {
            return document.documentElement.dataset.theme === darkTheme;
        },
        toggle() {
            const theme = this.isDark() ? lightTheme : darkTheme;
            localStorage.setItem(storageKey, theme);
            apply(theme);
            return this.isDark();
        }
    };
})();
