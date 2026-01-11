// Modern Navbar Interactive Features for SGP Freelancing Platform

(function () {
  "use strict";

  // Wait for DOM to be fully loaded
  document.addEventListener("DOMContentLoaded", function () {
    initNavbar();
  });

  function initNavbar() {
    const navbar = document.querySelector(".modern-navbar");
    if (!navbar) return;

    // Initialize all features
    handleScrollEffects(navbar);
    highlightActiveLink();
    enhanceSearchBar();
    handleMobileMenu();
    initNotifications();
  }

  // Add scroll effects to navbar
  function handleScrollEffects(navbar) {
    let lastScroll = 0;

    window.addEventListener("scroll", function () {
      const currentScroll = window.pageYOffset;

      // Add 'scrolled' class when user scrolls down
      if (currentScroll > 50) {
        navbar.classList.add("scrolled");
      } else {
        navbar.classList.remove("scrolled");
      }

      // Optional: Hide navbar on scroll down, show on scroll up
      // Uncomment if you want this feature
      /*
            if (currentScroll > lastScroll && currentScroll > 100) {
                navbar.style.transform = 'translateY(-100%)';
            } else {
                navbar.style.transform = 'translateY(0)';
            }
            */

      lastScroll = currentScroll;
    });
  }

  // Highlight active navigation link based on current page
  function highlightActiveLink() {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll(".navbar-nav .nav-link");

    navLinks.forEach((link) => {
      const href = link.getAttribute("href");

      // Remove existing active class
      link.classList.remove("active");

      // Add active class to matching link
      if (href && currentPath.includes(href) && href !== "/") {
        link.classList.add("active");
      } else if (href === "/" && currentPath === "/") {
        link.classList.add("active");
      }
    });
  }

  // Enhance search bar functionality
  function enhanceSearchBar() {
    const searchInput = document.querySelector(".navbar-search input");
    if (!searchInput) return;

    // Add search functionality
    searchInput.addEventListener("keypress", function (e) {
      if (e.key === "Enter") {
        const searchTerm = this.value.trim();
        if (searchTerm) {
          // Redirect to search results page
          window.location.href = `/Project/Index?search=${encodeURIComponent(
            searchTerm
          )}`;
        }
      }
    });

    // Add clear button functionality (if added in future)
    searchInput.addEventListener("input", function () {
      if (this.value.length > 0) {
        this.parentElement.classList.add("has-value");
      } else {
        this.parentElement.classList.remove("has-value");
      }
    });

    // Search suggestions (placeholder for future enhancement)
    let searchTimeout;
    searchInput.addEventListener("input", function () {
      clearTimeout(searchTimeout);
      const searchTerm = this.value.trim();

      if (searchTerm.length >= 3) {
        searchTimeout = setTimeout(() => {
          // Future: Fetch and display search suggestions
          console.log("Searching for:", searchTerm);
        }, 300);
      }
    });
  }

  // Handle mobile menu interactions
  function handleMobileMenu() {
    const navbarToggler = document.querySelector(".navbar-toggler");
    const navbarCollapse = document.querySelector(".navbar-collapse");

    if (!navbarToggler || !navbarCollapse) return;

    // Close mobile menu when clicking outside
    document.addEventListener("click", function (event) {
      const isClickInside =
        navbarCollapse.contains(event.target) ||
        navbarToggler.contains(event.target);

      if (!isClickInside && navbarCollapse.classList.contains("show")) {
        navbarToggler.click();
      }
    });

    // Close mobile menu when clicking on a link
    const navLinks = navbarCollapse.querySelectorAll(".nav-link");
    navLinks.forEach((link) => {
      link.addEventListener("click", function () {
        if (
          window.innerWidth < 992 &&
          navbarCollapse.classList.contains("show")
        ) {
          setTimeout(() => {
            navbarToggler.click();
          }, 200);
        }
      });
    });
  }

  // Initialize notification functionality
  function initNotifications() {
    const notificationBadges = document.querySelectorAll(".notification-badge");

    notificationBadges.forEach((badge) => {
      // Add click handler for notifications
      const notificationLink = badge.closest(".nav-link");
      if (notificationLink) {
        notificationLink.addEventListener("click", function (e) {
          // Future: Show notification dropdown or redirect to notifications page
          console.log("Notifications clicked");
        });
      }
    });

    // Simulate notification updates (for demo purposes)
    // In production, this would be replaced with SignalR or WebSocket updates
    updateNotificationCount();
  }

  // Update notification counts (placeholder for real-time updates)
  function updateNotificationCount() {
    // This would typically be replaced with SignalR hub connection
    // Example:
    /*
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .build();
        
        connection.on("ReceiveNotification", function(count) {
            updateBadgeCount('.notification-badge', count);
        });
        */
  }

  // Helper function to update badge count
  function updateBadgeCount(selector, count) {
    const badges = document.querySelectorAll(selector);
    badges.forEach((badge) => {
      if (count > 0) {
        badge.textContent = count > 99 ? "99+" : count;
        badge.style.display = "block";
      } else {
        badge.style.display = "none";
      }
    });
  }

  // Smooth scroll to sections (if using anchor links)
  function initSmoothScroll() {
    const anchorLinks = document.querySelectorAll('a[href^="#"]');

    anchorLinks.forEach((link) => {
      link.addEventListener("click", function (e) {
        const targetId = this.getAttribute("href");
        if (targetId === "#") return;

        const targetElement = document.querySelector(targetId);
        if (targetElement) {
          e.preventDefault();
          const navbarHeight =
            document.querySelector(".modern-navbar").offsetHeight;
          const targetPosition = targetElement.offsetTop - navbarHeight - 20;

          window.scrollTo({
            top: targetPosition,
            behavior: "smooth",
          });
        }
      });
    });
  }

  // Add keyboard navigation support
  function initKeyboardNavigation() {
    const dropdowns = document.querySelectorAll(".dropdown-toggle");

    dropdowns.forEach((dropdown) => {
      dropdown.addEventListener("keydown", function (e) {
        if (e.key === "Enter" || e.key === " ") {
          e.preventDefault();
          this.click();
        }
      });
    });
  }

  // Handle user avatar generation (if not set from server)
  function generateUserAvatars() {
    const userAvatars = document.querySelectorAll(".user-avatar");

    userAvatars.forEach((avatar) => {
      if (!avatar.textContent.trim()) {
        const username =
          document.querySelector("#userDropdown span")?.textContent || "User";
        avatar.textContent = username.charAt(0).toUpperCase();
      }

      // Generate a consistent color based on username
      const colors = [
        "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
        "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
        "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)",
        "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)",
        "linear-gradient(135deg, #fa709a 0%, #fee140 100%)",
      ];

      const username =
        document.querySelector("#userDropdown span")?.textContent || "User";
      const colorIndex = username.charCodeAt(0) % colors.length;
      avatar.style.background = colors[colorIndex];
    });
  }

  // Initialize smooth scroll and keyboard navigation
  initSmoothScroll();
  initKeyboardNavigation();
  generateUserAvatars();

  // Export functions for external use
  window.NavbarUtils = {
    updateNotificationCount: updateBadgeCount,
    highlightActiveLink: highlightActiveLink,
  };
})();
