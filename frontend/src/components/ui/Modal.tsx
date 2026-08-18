import React, { useEffect } from 'react';
import { X } from 'lucide-react';
import { useThemeStore } from '../../store/themeStore';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  size?: 'sm' | 'md' | 'lg' | 'xl' | '2xl';
}

const sizeClasses = {
  sm: 'max-w-md',
  md: 'max-w-lg',
  lg: 'max-w-2xl',
  xl: 'max-w-4xl',
  '2xl': 'max-w-5xl',
};

export const Modal: React.FC<ModalProps> = ({ isOpen, onClose, title, children, size = 'md' }) => {
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="fixed inset-0 bg-black/60 backdrop-blur-sm transition-opacity" onClick={onClose} />
        <div className={`relative w-full ${sizeClasses[size]} transform rounded-2xl shadow-2xl transition-all ${
          isDark ? 'bg-gray-800 border border-gray-700' : 'bg-white'
        }`}>
          <div className={`flex items-center justify-between px-6 py-4 ${
            isDark ? 'border-b border-gray-700' : 'border-b border-gray-200'
          }`}>
            <h3 className={`text-lg font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{title}</h3>
            <button
              onClick={onClose}
              className={`rounded-lg p-2 transition-colors ${
                isDark ? 'hover:bg-gray-700 text-gray-400 hover:text-white' : 'hover:bg-gray-100 text-gray-500 hover:text-gray-700'
              }`}
            >
              <X className="h-5 w-5" />
            </button>
          </div>
          <div className="px-6 py-5 max-h-[70vh] overflow-y-auto">
            {children}
          </div>
        </div>
      </div>
    </div>
  );
};
