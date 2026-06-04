/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      // Charte « Jewellery Shop » (mappée sur les variables CSS de :root)
      colors: {
        storm: 'var(--storm)',
        slate: 'var(--slate)',
        sky: 'var(--sky)',
        mist: 'var(--mist)',
        fuchsia: 'var(--fuchsia)',
        flamant: 'var(--flamant)',
        poudre: 'var(--poudre)',
        blush: 'var(--blush)',
        creme: 'var(--creme)',
        lin: 'var(--lin)',
        encre: 'var(--encre)',
      },
      fontFamily: {
        display: ['Cormorant Garamond', 'serif'],
        body: ['Jost', 'sans-serif'],
        comfortaa: ['Comfortaa', 'cursive'],
      },
      animation: {
        'slide-up': 'slide-up 0.5s ease-out both',
        'fade-in': 'fade-in 0.3s ease both',
      },
      boxShadow: {
        focus: '0 0 0 3px rgba(240,71,138,0.08)',
        card: '0 4px 24px rgba(56,73,89,0.06), 0 1px 3px rgba(56,73,89,0.04)',
        btn: '0 4px 14px rgba(56,73,89,0.2)',
        'btn-hover': '0 6px 20px rgba(56,73,89,0.25)',
        toggle: '0 2px 8px rgba(0,0,0,0.06)',
      },
    },
  },
  plugins: [],
};
