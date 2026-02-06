# Tailwind CSS Migration Guide

## ✅ Completed Changes

### 1. Tailwind CSS Installation

- **Installed**: Tailwind CSS via npm (`npm install -D tailwindcss`)
- **Config Created**: `tailwind.config.js` with custom colors (indigo/purple theme)
- **Input CSS**: `wwwroot/css/tailwind-input.css` with custom components
- **Output CSS**: `wwwroot/css/tailwind-output.css` (compiled)
- **Build Command**: `npm run build:css`
- **Watch Command**: `npm run watch:css` (for development)

### 2. Bootstrap Removal

- ✅ Removed Bootstrap CSS/JS references from `_Layout.cshtml`
- ✅ Deleted `wwwroot/lib/bootstrap` directory
- ✅ Removed Bootstrap script references

### 3. Main Layout Updated

- ✅ `Views/Shared/_Layout.cshtml` - Fully migrated to Tailwind
  - Modern responsive navbar with Tailwind classes
  - Mobile menu toggle
  - User dropdown menu
  - Footer updated to Tailwind grid system

## 🎨 Tailwind Custom Classes Available

### Buttons

- `.btn-primary` - Indigo button with hover effects
- `.btn-secondary` - Gray button
- `.btn-outline` - Outlined button

### Cards

- `.card` - White card with shadow and rounded corners

### Inputs

- `.input-field` - Styled input with focus states

### Badges

- `.badge`, `.badge-success`, `.badge-warning`, `.badge-danger`, `.badge-info`

### Utilities

- `.text-gradient` - Gradient text effect (indigo to purple)
- `.glass-effect` - Glassmorphism backdrop effect

## 📋 Migration TODO for Individual Pages

### Views to Update (Bootstrap → Tailwind)

#### High Priority

1. **Home/Index.cshtml** - Landing page with hero section
2. **Dashboard/Index.cshtml** - Main dashboard
3. **Project/Index.cshtml** - Project listing
4. **Project/Create.cshtml** - Project creation form
5. **Project/Details.cshtml** - Project details page

#### Medium Priority

6. **Account/Login.cshtml** - Login form
7. **Account/Register.cshtml** - Registration form
8. **Message/Index.cshtml** - Messages page
9. **Settings/Index.cshtml** - Settings page
10. **Earnings/Index.cshtml** - Earnings page

## 📖 Common Bootstrap to Tailwind Conversions

### Layout

```
Bootstrap → Tailwind
---------------------------------
container → container mx-auto px-4
container-fluid → w-full px-4
row → flex flex-wrap / grid
col-md-6 → md:w-1/2 / md:col-span-6
col-lg-4 → lg:w-1/3 / lg:col-span-4
```

### Spacing

```
Bootstrap → Tailwind
---------------------------------
mt-3 → mt-4
mb-4 → mb-6
p-3 → p-4
px-4 → px-6
py-5 → py-8
gap-3 → gap-4
```

### Buttons

```
Bootstrap → Tailwind
---------------------------------
btn btn-primary → btn-primary (custom) or bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2 px-4 rounded-lg
btn btn-secondary → btn-secondary (custom) or bg-gray-600 hover:bg-gray-700 text-white py-2 px-4 rounded-lg
btn btn-outline-primary → border-2 border-indigo-600 text-indigo-600 hover:bg-indigo-600 hover:text-white py-2 px-4 rounded-lg
btn-lg → py-3 px-6 text-lg
btn-sm → py-1 px-3 text-sm
```

### Cards

```
Bootstrap → Tailwind
---------------------------------
card → bg-white rounded-xl shadow-md
card-body → p-6
card-title → text-xl font-bold mb-4
card-text → text-gray-600
```

### Forms

```
Bootstrap → Tailwind
---------------------------------
form-control → input-field (custom) or w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500
form-label → block text-sm font-medium text-gray-700 mb-2
form-group → mb-4
input-group → flex
```

### Typography

```
Bootstrap → Tailwind
---------------------------------
h1 → text-4xl font-bold
h2 → text-3xl font-bold
h3 → text-2xl font-semibold
lead → text-xl text-gray-600
text-muted → text-gray-500
text-center → text-center
fw-bold → font-bold
```

### Display & Flexbox

```
Bootstrap → Tailwind
---------------------------------
d-flex → flex
d-none → hidden
d-block → block
d-lg-block → lg:block
justify-content-between → justify-between
align-items-center → items-center
flex-column → flex-col
flex-row → flex-row
```

### Colors

```
Bootstrap → Tailwind
---------------------------------
text-primary → text-indigo-600
text-success → text-green-600
text-danger → text-red-600
text-warning → text-yellow-600
bg-light → bg-gray-50
bg-dark → bg-gray-900
```

### Utilities

```
Bootstrap → Tailwind
---------------------------------
rounded → rounded-lg
shadow → shadow-md
shadow-lg → shadow-xl
mb-3 → mb-4
mt-5 → mt-8
w-100 → w-full
mx-auto → mx-auto
```

## 🔧 Development Workflow

### Building Tailwind CSS

```bash
# Build once (production)
npm run build:css

# Watch for changes (development)
npm run watch:css
```

### Important Notes

1. **Always run `npm run build:css`** after modifying `tailwind-input.css`
2. **Keep `tailwind.config.js`** updated with custom colors and utilities
3. **Use custom classes** from `tailwind-input.css` for consistency
4. **Don't mix Bootstrap and Tailwind** - Complete migration required

## 🎨 Color Palette

### Primary Colors

- `indigo-500`: #4F46E5 (main brand color)
- `indigo-600`: #4338CA
- `indigo-700`: #3730A3
- `purple-600`: #7C3AED (secondary)
- `purple-700`: #6D28D9

### Usage

```html
<!-- Backgrounds -->
<div class="bg-indigo-600 hover:bg-indigo-700">
  <!-- Text -->
  <h1 class="text-indigo-600">
    <!-- Borders -->
    <div class="border-2 border-indigo-600">
      <!-- Gradients -->
      <div class="bg-gradient-to-r from-indigo-600 to-purple-600"></div>
    </div>
  </h1>
</div>
```

## 📝 Example Page Migration

### Before (Bootstrap):

```html
<div class="container">
  <div class="row">
    <div class="col-md-6">
      <div class="card">
        <div class="card-body">
          <h3 class="card-title">Title</h3>
          <p class="card-text text-muted">Content</p>
          <button class="btn btn-primary">Click</button>
        </div>
      </div>
    </div>
  </div>
</div>
```

### After (Tailwind):

```html
<div class="container mx-auto px-4">
  <div class="grid md:grid-cols-2 gap-4">
    <div class="bg-white rounded-xl shadow-md p-6">
      <h3 class="text-2xl font-bold mb-4">Title</h3>
      <p class="text-gray-500 mb-4">Content</p>
      <button class="btn-primary">Click</button>
    </div>
  </div>
</div>
```

## 🚀 Next Steps

1. Update individual view files as needed
2. Test responsive behavior on mobile devices
3. Verify all forms work correctly
4. Check dropdown menus and modals
5. Test authentication pages
6. Verify all interactive elements

## ⚠️ Breaking Changes

1. **No Bootstrap JavaScript** - Custom JS needed for:
   - Dropdowns (implemented with vanilla JS)
   - Modals (need custom implementation)
   - Tooltips (need custom implementation or library)
   - Carousels (need custom implementation or library)

2. **Grid System Changed** - Use Tailwind's grid or flexbox
3. **Utility Classes Different** - Refer to conversion table above

## 📚 Resources

- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [Tailwind CSS IntelliSense](https://marketplace.visualstudio.com/items?itemName=bradlc.vscode-tailwindcss) (VS Code Extension)
- [Headless UI](https://headlessui.com/) (for complex components)
- [DaisyUI](https://daisyui.com/) (optional component library)
