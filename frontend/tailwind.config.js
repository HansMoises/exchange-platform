/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#2E5D34',
          hover: '#1F3F24',
          soft: '#DCE8DD',
          100: '#EAF2EA',
        },
        secondary: {
          DEFAULT: '#6B7F3A',
          hover: '#556630',
        },
        earth: {
          DEFAULT: '#A6764F',
          soft: '#C8A27C',
        },
        bg: {
          DEFAULT: '#FFFFFF',
          alt: '#F6F5F1',
          warm: '#EDE7DD',
        },
        text: {
          DEFAULT: '#33332F',
          secondary: '#66665F',
        },
        success: '#2E7D4F',
        warning: '#B8860B',
        error: '#B23B3B',
        info: '#2C6E91',
      },
      borderRadius: {
        sm: '8px',
        md: '14px',
        lg: '20px',
        xl: '28px',
      },
      boxShadow: {
        // Sombra base para tarjetas en reposo (mas suave y en capas que un solo blur).
        card: '0 1px 2px rgba(51,51,47,0.04), 0 2px 8px rgba(51,51,47,0.06)',
        soft: '0 1px 2px rgba(51,51,47,0.05)',
        // Estado hover/elevado de tarjetas y paneles.
        elevated: '0 16px 32px -12px rgba(51,51,47,0.18), 0 4px 10px -4px rgba(51,51,47,0.08)',
        // Resplandor de marca para CTAs principales en hover/foco.
        glow: '0 10px 28px -8px rgba(46,93,52,0.45)',
        glass: '0 8px 32px rgba(51,51,47,0.10)',
      },
      spacing: {
        xs: '4px',
        sm: '8px',
        md: '16px',
        lg: '24px',
        xl: '32px',
        '2xl': '48px',
        '3xl': '64px',
      },
      fontFamily: {
        base: ['Inter', 'system-ui', 'sans-serif'],
        display: ['"Plus Jakarta Sans"', 'Inter', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        hero: ['3rem', { fontWeight: '800', lineHeight: '1.1', letterSpacing: '-0.02em' }],
        h1: ['2rem', { fontWeight: '700', letterSpacing: '-0.01em' }],
        h2: ['1.625rem', { fontWeight: '600', letterSpacing: '-0.01em' }],
        h3: ['1.3125rem', { fontWeight: '600' }],
        body: ['1rem', { fontWeight: '400' }],
        'body-s': ['0.875rem', { fontWeight: '400' }],
        caption: ['0.75rem', { fontWeight: '400' }],
        label: ['0.875rem', { fontWeight: '500' }],
      },
      screens: {
        sm: '640px',
        md: '768px',
        lg: '1024px',
        xl: '1280px',
      },
      keyframes: {
        fadeIn: {
          from: { opacity: '0' },
          to: { opacity: '1' },
        },
        slideUp: {
          from: { opacity: '0', transform: 'translateY(12px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
        scaleIn: {
          from: { opacity: '0', transform: 'scale(0.96)' },
          to: { opacity: '1', transform: 'scale(1)' },
        },
      },
      animation: {
        'fade-in': 'fadeIn 250ms ease-out both',
        'slide-up': 'slideUp 300ms ease-out both',
        'scale-in': 'scaleIn 200ms ease-out both',
      },
      transitionDuration: {
        DEFAULT: '200ms',
      },
    },
  },
  plugins: [],
};
