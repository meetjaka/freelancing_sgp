# Tailwind CSS Quick Reference for SGP Freelancing

## Project Colors

```css
Primary: indigo-600 (#4F46E5)
Secondary: purple-600 (#7C3AED)
Accent: blue-600 (#3B82F6)
```

## Common Patterns

### Container

```html
<div class="container mx-auto px-4"></div>
```

### Grid Layout (2 columns)

```html
<div class="grid md:grid-cols-2 gap-6"></div>
```

### Card

```html
<div
  class="bg-white rounded-xl shadow-md p-6 hover:shadow-lg transition-shadow"
></div>
```

### Primary Button

```html
<button
  class="bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2 px-6 rounded-lg transition-all duration-200 shadow-md hover:shadow-lg"
></button>
```

### Input Field

```html
<input
  type="text"
  class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
/>
```

### Badge

```html
<span
  class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-green-100 text-green-800"
></span>
```

### Gradient Background

```html
<div class="bg-gradient-to-r from-indigo-600 to-purple-600"></div>
```

### Responsive Padding

```html
<div class="px-4 py-8 md:px-8 lg:px-16"></div>
```

## Build Commands

```bash
# Build CSS (run after changes)
npm run build:css

# Watch for changes (development)
npm run watch:css
```

## Custom Classes (from tailwind-input.css)

- `.btn-primary` - Styled primary button
- `.btn-secondary` - Styled secondary button
- `.btn-outline` - Outlined button
- `.card` - Card component
- `.input-field` - Styled input field
- `.badge-success` - Green badge
- `.badge-warning` - Yellow badge
- `.badge-danger` - Red badge
- `.badge-info` - Blue badge
- `.text-gradient` - Gradient text effect
- `.glass-effect` - Glassmorphism effect
