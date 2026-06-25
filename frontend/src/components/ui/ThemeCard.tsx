import React from 'react';
import { useThemeStore } from '../../store/themeStore';

interface ThemeCardProps {
  children: React.ReactNode;
  className?: string;
  onClick?: () => void;
}

export const ThemeCard: React.FC<ThemeCardProps> = ({ children, className = '', onClick }) => {
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  return (
    <div
      onClick={onClick}
      className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border ${className}`}
    >
      {children}
    </div>
  );
};

export const useThemeClasses = () => {
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  return {
    isDark,
    bg: isDark ? 'bg-gray-900' : 'bg-gray-50',
    card: isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100',
    cardBg: isDark ? 'bg-gray-800' : 'bg-white',
    input: isDark
      ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-400 focus:ring-emerald-500 focus:border-emerald-500'
      : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400 focus:ring-blue-500 focus:border-blue-500',
    text: isDark ? 'text-white' : 'text-gray-900',
    textSecondary: isDark ? 'text-gray-400' : 'text-gray-500',
    textMuted: isDark ? 'text-gray-500' : 'text-gray-400',
    tableHeader: isDark ? 'bg-gray-700' : 'bg-gray-50',
    tableRow: isDark ? 'hover:bg-gray-700/50 border-gray-700' : 'hover:bg-gray-50 border-gray-100',
    tableCell: isDark ? 'text-gray-300' : 'text-gray-900',
    hoverBg: isDark ? 'hover:bg-gray-700' : 'hover:bg-gray-100',
    border: isDark ? 'border-gray-700' : 'border-gray-200',
    select: isDark
      ? 'bg-gray-700 border-gray-600 text-white focus:ring-emerald-500'
      : 'bg-white border-gray-300 text-gray-900 focus:ring-blue-500',
    badge: {
      success: isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-700',
      warning: isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-700',
      danger: isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-700',
      info: isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-700',
    },
  };
};
