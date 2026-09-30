import React from 'react';

interface ResponsiveTableProps {
  children: React.ReactNode;
  /** Minimum table width before it starts scrolling horizontally. */
  minWidthClass?: string;
  className?: string;
}

/**
 * Wraps a wide table so it scrolls inside its own container instead of forcing
 * the page wider than the viewport. Without this the table's intrinsic min
 * content width pushes the whole layout sideways on phones.
 */
export const ResponsiveTable: React.FC<ResponsiveTableProps> = ({
  children,
  minWidthClass = 'min-w-[640px]',
  className = '',
}) => (
  <div className={`w-full overflow-x-auto -webkit-overflow-scrolling:touch [scrollbar-width:thin] ${className}`}>
    <div className={`w-full ${minWidthClass}`}>{children}</div>
  </div>
);

/**
 * Horizontal cell padding that tightens on narrow screens so wide tables stay
 * readable without forcing a horizontal scroll at 320px.
 */
export const cellPad = 'px-3 sm:px-6';
