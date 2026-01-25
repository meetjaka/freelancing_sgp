// Professional Modern Navbar Interactive Features for SGP Freelancing Platform

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
    addSmoothScrolling();
  }

  // Enhanced scroll effects to navbar with smooth transitions
  function handleScrollEffects(navbar) {
    let lastScroll = 0;
    let ticking = false;

    window.addEventListener("scroll", function () {
      if (!ticking) {
        window.requestAnimationFrame(function () {
          const currentScroll = window.pageYOffset;

          // Add 'scrolled' class when user scrolls down
          if (currentScroll > 50) {
            navbar.classList.add("scrolled");
          } else {
            navbar.classList.remove("scrolled");
          }

          // Add shadow effect based on scroll position
          const shadowIntensity = Math.min(currentScroll / 100, 1);
          navbar.style.boxShadow = `0 ${8 + shadowIntensity * 8}px ${
            32 + shadowIntensity * 16
          }px rgba(31, 38, 135, ${0.15 + shadowIntensity * 0.1})`;

          lastScroll = currentScroll;
          ticking = false;
        });

        ticking = true;
      }
    });

    // Add initial animation on page load
    setTimeout(() => {
      navbar.style.opacity = "1";
      navbar.style.transform = "translateY(0)";
    }, 100);
  }

  // Highlight active navigation link based on current page with smooth transitions
  function highlightActiveLink() {
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll(".navbar-nav .nav-link");

    navLinks.forEach((link) => {
      const href = link.getAttribute("href");

      // Remove existing active class
      link.classList.remove("active");

      // Add active class to matching link with improved logic
      if (href) {
        const linkPath = href.toLowerCase();
        if (currentPath === linkPath) {
          link.classList.add("active");
        } else if (linkPath !== "/" && currentPath.startsWith(linkPath)) {
          link.classList.add("active");
        }
      }
    });

    // Add click effect to all nav links
    navLinks.forEach((link) => {
      link.addEventListener("click", function (e) {
        // Add ripple effect
        createRipple(e, this);
      });
    });
  }

  // Create ripple effect on click
  function createRipple(event, element) {
    const ripple = document.createElement("span");
    const rect = element.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left - size / 2;
    const y = event.clientY - rect.top - size / 2;

    ripple.style.cssText = `
            width: ${size}px;
            height: ${size}px;
            left: ${x}px;
            top: ${y}px;
            position: absolute;
            background: rgba(255, 255, 255, 0.3);
            border-radius: 50%;
            transform: scale(0);
            animation: ripple 0.6s ease-out;
            pointer-events: none;
        `;

    element.style.position = "relative";
    element.style.overflow = "hidden";
    element.appendChild(ripple);

    setTimeout(() => ripple.remove(), 600);
  }

  // Enhanced search bar functionality with animations
  function enhanceSearchBar() {
    const searchInput = document.querySelector(".navbar-search input");
    const searchIcon = document.querySelector(".navbar-search .search-icon");
    if (!searchInput) return;

    // Add search functionality with Enter key
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

    // Add focus animations
    searchInput.addEventListener("focus", function () {
      this.parentElement.classList.add("focused");
    });

    searchInput.addEventListener("blur", function () {
      if (!this.value) {
        this.parentElement.classList.remove("focused");
      }
    });

    // Add value tracking
    searchInput.addEventListener("input", function () {
      if (this.value.length > 0) {
        this.parentElement.classList.add("has-value");
      } else {
        this.parentElement.classList.remove("has-value");
      }
    });

    // Search suggestions with debouncing
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

  // Handle mobile menu interactions with smooth animations
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
  function addSmoothScrolling() {
    const anchorLinks = document.querySelectorAll('a[href^="#"]');

    anchorLinks.forEach((link) => {
      link.addEventListener("click", function (e) {
        const targetId = this.getAttribute("href");
        if (targetId === "#" || !targetId) return;

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

    // Add keyboard navigation support
    initKeyboardNavigation();
    
    // Initialize user avatar enhancements
    enhanceUserDropdown();
  }

  // Add keyboard navigation support for accessibility
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

  // Enhanced user dropdown with animations
  function enhanceUserDropdown() {
    const userDropdown = document.querySelector(".user-dropdown-toggle");
    if (!userDropdown) return;

    // Add smooth opening animation
    userDropdown.addEventListener("show.bs.dropdown", function () {
      this.classList.add("dropdown-opening");
    });

    userDropdown.addEventListener("shown.bs.dropdown", function () {
      this.classList.remove("dropdown-opening");
    });
  }

  // Add ripple animation keyframe
  const style = document.createElement("style");
  style.textContent = `
        @keyframes ripple {
            to {
                transform: scale(4);
                opacity: 0;
            }
        }
    `;
  document.head.appendChild(style);
})();
  initSmoothScroll();
  initKeyboardNavigation();
  generateUserAvatars();

  // Export functions for external use
  window.NavbarUtils = {
    updateNotificationCount: updateBadgeCount,
    highlightActiveLink: highlightActiveLink,
  };
})();
