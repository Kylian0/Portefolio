window.portfolioMotion = (() => {
    let observer;

    const selectors = [
        ".home-page .hero__content",
        ".home-page .hero__visual",
        ".home-page .home-section",
        ".home-page .home-cta",
        ".projects-header",
        ".projects-grid .project-card",
        ".contact-header",
        ".contact-layout > *",
        ".project-document > *"
    ].join(",");

    function clear() {
        observer?.disconnect();
        observer = undefined;
        document.querySelectorAll(".motion-reveal").forEach(element => {
            element.classList.remove("motion-reveal", "motion-visible");
            element.style.removeProperty("--motion-delay");
        });
    }

    function initialize(enabled) {
        clear();

        if (!enabled || window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            return;
        }

        requestAnimationFrame(() => {
            const elements = [...document.querySelectorAll(selectors)];

            observer = new IntersectionObserver(entries => {
                entries.forEach(entry => {
                    if (!entry.isIntersecting) return;
                    entry.target.classList.add("motion-visible");
                    observer.unobserve(entry.target);
                });
            }, { threshold: 0.12, rootMargin: "0px 0px -40px" });

            elements.forEach((element, index) => {
                element.classList.add("motion-reveal");
                element.style.setProperty("--motion-delay", `${Math.min(index % 4, 3) * 55}ms`);
                observer.observe(element);
            });
        });
    }

    return { initialize };
})();
