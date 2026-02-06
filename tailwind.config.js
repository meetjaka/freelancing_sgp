/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml",
    "./wwwroot/**/*.html",
    "./wwwroot/js/**/*.js",
    "./Areas/**/*.cshtml",
    "./**/*.razor"
  ],
  safelist: [
    "bg-gradient-to-r",
    "bg-gradient-to-br",
    "from-indigo-600",
    "to-purple-600",
    "from-slate-50",
    "via-blue-50",
    "to-indigo-50",
    "backdrop-blur-lg",
    "backdrop-filter",
    "text-slate-700",
    "text-slate-900",
    "bg-slate-100",
    "bg-slate-800",
    "bg-slate-900",
    "border-slate-200",
    "hover:bg-indigo-50",
    "hover:text-indigo-600",
    "group-hover:scale-110",
    "group-hover:text-indigo-600",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: "#f5f3ff",
          100: "#ede9fe",
          200: "#ddd6fe",
          300: "#c4b5fd",
          400: "#a78bfa",
          500: "#8b5cf6",
          600: "#7c3aed",
          700: "#6d28d9",
          800: "#5b21b6",
          900: "#4c1d95",
        },
      },
      fontFamily: {
        sans: ["Inter", "system-ui", "sans-serif"],
      },
      boxShadow: {
        soft: "0 2px 15px rgba(0, 0, 0, 0.08)",
        medium: "0 4px 20px rgba(0, 0, 0, 0.1)",
        strong: "0 8px 32px rgba(79, 70, 229, 0.25)",
      },
    },
  },
  plugins: [],
};
