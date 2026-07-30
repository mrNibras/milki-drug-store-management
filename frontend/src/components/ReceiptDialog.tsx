import React from 'react';
import { Printer, X } from 'lucide-react';
import { Modal } from './ui/Modal';
import { Button } from './ui/Button';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Sale } from '../types';
import { formatCurrency } from '../utils/helpers';

interface ReceiptDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  sale: Sale | null;
}

export const ReceiptDialog: React.FC<ReceiptDialogProps> = ({ open, onOpenChange, sale }) => {
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const { settings } = useAppStore();

  const handlePrint = () => {
    window.print();
  };

  if (!sale) return null;

  const subtotal = sale.totalDiscount > 0 ? sale.totalAmount + sale.totalDiscount : sale.totalAmount;

  return (
    <Modal isOpen={open} onClose={() => onOpenChange(false)} title="Receipt" size="md">
      <style>{`
        @media print {
          body * {
            visibility: hidden;
          }
          .receipt-print-area, .receipt-print-area * {
            visibility: visible;
          }
          .receipt-print-area {
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
            background: white !important;
            color: black !important;
            padding: 20px;
          }
          .no-print {
            display: none !important;
          }
          .receipt-print-area table {
            border-collapse: collapse;
            width: 100%;
          }
          .receipt-print-area th, .receipt-print-area td {
            border-bottom: 1px solid #ddd;
            padding: 4px 8px;
            text-align: left;
          }
        }
      `}</style>

      <div className={`receipt-print-area ${isDark ? 'text-gray-300' : 'text-gray-900'}`}>
        <div className="text-center mb-6">
          <h2 className="text-xl font-bold">{settings.pharmacyName}</h2>
          <p className="text-sm text-gray-500">{settings.address}</p>
          <p className="text-sm text-gray-500">Tel: {settings.phone}</p>
          <p className="text-xs text-gray-400 mt-1">Email: {settings.email}</p>
        </div>

        <div className={`border-b pb-4 mb-4 ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
          <div className="flex justify-between text-sm mb-1">
            <span>Receipt #:</span>
            <span className="font-medium">{sale.saleNumber}</span>
          </div>
          <div className="flex justify-between text-sm mb-1">
            <span>Date:</span>
            <span className="font-medium">{new Date(sale.saleDate).toLocaleString()}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span>Cashier:</span>
            <span className="font-medium">{sale.userName}</span>
          </div>
        </div>

        <table className="w-full border-collapse mb-4">
          <thead>
            <tr className={`border-b-2 ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
              <th className="text-left text-sm pb-2 font-medium">Item</th>
              <th className="text-center text-sm pb-2 font-medium w-16">Qty</th>
              <th className="text-right text-sm pb-2 font-medium w-20">Price</th>
              <th className="text-right text-sm pb-2 font-medium w-20">Total</th>
            </tr>
          </thead>
          <tbody>
            {sale.items.map((item, idx) => (
              <tr key={item.id || idx} className={`border-b ${isDark ? 'border-gray-700' : 'border-gray-100'}`}>
                <td className="py-2 text-sm">{item.brandName}</td>
                <td className="py-2 text-sm text-center">{item.quantity}</td>
                <td className="py-2 text-sm text-right">{formatCurrency(item.unitPrice)}</td>
                <td className="py-2 text-sm text-right">{formatCurrency(item.totalPrice)}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <div className={`border-t pt-3 space-y-1 ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
          <div className="flex justify-between text-sm">
            <span>Subtotal</span>
            <span>{formatCurrency(subtotal)}</span>
          </div>
          {sale.totalDiscount > 0 && (
            <div className="flex justify-between text-sm text-red-500">
              <span>Discount</span>
              <span>-{formatCurrency(sale.totalDiscount)}</span>
            </div>
          )}
          {sale.discountReason && (
            <div className="text-xs text-gray-500">Reason: {sale.discountReason}</div>
          )}
          <div className="flex justify-between text-lg font-bold pt-2">
            <span>Total</span>
            <span>{formatCurrency(sale.totalAmount)}</span>
          </div>
          <div className="flex justify-between text-sm pt-2">
            <span>Payment Method</span>
            <span className="font-medium">Cash</span>
          </div>
        </div>

        <div className="no-print flex gap-3 mt-6 pt-4">
          <Button variant="secondary" onClick={() => onOpenChange(false)} className="flex-1">
            <X className="h-4 w-4" /> Close
          </Button>
          <Button variant="primary" onClick={handlePrint} className="flex-1">
            <Printer className="h-4 w-4" /> Print
          </Button>
        </div>
      </div>
    </Modal>
  );
};
