import React, { useState, useEffect } from 'react';
import { DollarSign, CreditCard } from 'lucide-react';
import { Modal } from '../ui/Modal';
import { Badge } from '../ui/Badge';
import { formatDate, formatCurrency } from '../../utils/helpers';
import { supplierPaymentService, SupplierDebtSummaryResponse, SupplierPaymentResponse } from '../../services/supplierPaymentService';

interface SupplierDebtModalProps {
  isOpen: boolean;
  onClose: () => void;
  supplier: { id: string; name: string; phone: string } | null;
  isDark: boolean;
  onPaymentSuccess: () => void;
}

export const SupplierDebtModal: React.FC<SupplierDebtModalProps> = ({ isOpen, onClose, supplier, isDark, onPaymentSuccess }) => {
  const [supplierDebt, setSupplierDebt] = useState<SupplierDebtSummaryResponse | null>(null);
  const [paymentHistory, setPaymentHistory] = useState<SupplierPaymentResponse[]>([]);
  const [showPayModal, setShowPayModal] = useState(false);
  const [payAmount, setPayAmount] = useState('');
  const [payMethod, setPayMethod] = useState('cash');
  const [payReference, setPayReference] = useState('');
  const [payNotes, setPayNotes] = useState('');
  const [payLoading, setPayLoading] = useState(false);
  const [payError, setPayError] = useState('');

  useEffect(() => {
    if (isOpen && supplier) {
      loadDebt();
    }
  }, [isOpen, supplier]);

  const loadDebt = async () => {
    if (!supplier) return;
    setPayError('');
    try {
      const debt = await supplierPaymentService.getSupplierDebt(parseInt(supplier.id));
      setSupplierDebt(debt);
      const history = await supplierPaymentService.getPaymentHistory(parseInt(supplier.id));
      setPaymentHistory(history);
    } catch (e: any) {
      setPayError(e.response?.data?.message || 'Failed to load debt information');
      setSupplierDebt(null);
      setPaymentHistory([]);
    }
  };

  const handlePayDebt = async () => {
    if (!supplier || !supplierDebt) return;
    const amount = parseFloat(payAmount);
    if (isNaN(amount) || amount <= 0) {
      setPayError('Please enter a valid payment amount');
      return;
    }
    if (amount > supplierDebt.outstandingDebt) {
      setPayError(`Payment exceeds the outstanding debt of ${formatCurrency(supplierDebt.outstandingDebt)}`);
      return;
    }
    let remaining = amount;
    const finalAllocations = [];
    for (const p of supplierDebt.outstandingPurchases) {
      if (remaining <= 0) break;
      const alloc = Math.min(remaining, p.amountDue);
      finalAllocations.push({ purchaseId: p.purchaseId, allocatedAmount: alloc });
      remaining -= alloc;
    }
    setPayLoading(true);
    setPayError('');
    try {
      await supplierPaymentService.recordPayment({
        supplierId: parseInt(supplier.id),
        branchId: 1,
        amount: amount,
        paymentDate: new Date().toISOString(),
        paymentMethod: payMethod,
        referenceNumber: payReference || undefined,
        notes: payNotes || undefined,
        allocations: finalAllocations
      });
      await loadDebt();
      setShowPayModal(false);
      setPayAmount('');
      setPayReference('');
      setPayNotes('');
      onPaymentSuccess();
    } catch (e: any) {
      setPayError(e.response?.data?.message || 'Failed to record payment');
    } finally {
      setPayLoading(false);
    }
  };

  const inputClass = `w-full px-3 py-2 rounded-xl border text-sm ${isDark ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-200 text-gray-900'} focus:outline-none focus:ring-2 focus:ring-blue-500`;

  return (
    <>
      <Modal isOpen={isOpen} onClose={onClose} title="Supplier Debt & Payments" size="lg">
        {supplier && (
          <div className="space-y-4">
            <div className={`p-4 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
              <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{supplier.name}</p>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{supplier.phone}</p>
            </div>
            {supplierDebt && (
              <div className="grid grid-cols-4 gap-3">
                <div className={`p-3 rounded-xl text-center ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Purchases</p>
                  <p className={`text-lg font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(supplierDebt.totalPurchases)}</p>
                </div>
                <div className={`p-3 rounded-xl text-center ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Paid</p>
                  <p className="text-lg font-bold text-green-500">{formatCurrency(supplierDebt.totalPaid)}</p>
                </div>
                <div className={`p-3 rounded-xl text-center ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Outstanding</p>
                  <p className={`text-lg font-bold ${supplierDebt.outstandingDebt > 0 ? 'text-red-500' : 'text-green-500'}`}>{formatCurrency(supplierDebt.outstandingDebt)}</p>
                </div>
                <div className={`p-3 rounded-xl text-center ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                  <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</p>
                  <p className={`text-sm font-bold ${supplierDebt.outstandingDebt > 0 ? 'text-red-500' : 'text-green-500'}`}>{supplierDebt.outstandingDebt > 0 ? 'Outstanding' : 'No Debt'}</p>
                </div>
              </div>
            )}
            {supplierDebt && supplierDebt.outstandingDebt > 0 && (
              <button onClick={() => setShowPayModal(true)} className="px-4 py-2 bg-gradient-to-r from-red-600 to-orange-600 text-white rounded-xl text-sm font-medium flex items-center gap-2">
                <CreditCard className="h-4 w-4" /> Pay Debt
              </button>
            )}
            {payError && <p className="text-red-500 text-sm">{payError}</p>}
            <div>
              <h4 className={`text-sm font-semibold mb-2 ${isDark ? 'text-white' : 'text-gray-900'}`}>Payment History</h4>
              <div className={`border rounded-xl overflow-hidden ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
                <table className="w-full text-sm">
                  <thead>
                    <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                      <th className={`text-left px-3 py-2 text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Date</th>
                      <th className={`text-right px-3 py-2 text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Amount</th>
                      <th className={`text-left px-3 py-2 text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Method</th>
                      <th className={`text-left px-3 py-2 text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Reference</th>
                    </tr>
                  </thead>
                  <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                    {paymentHistory.map(p => (
                      <tr key={p.supplierPaymentId}>
                        <td className={`px-3 py-2 ${isDark ? 'text-gray-300' : ''}`}>{formatDate(p.paymentDate)}</td>
                        <td className={`px-3 py-2 text-right ${isDark ? 'text-white' : ''}`}>{formatCurrency(p.amount)}</td>
                        <td className={`px-3 py-2 ${isDark ? 'text-gray-300' : ''}`}>{p.paymentMethod}</td>
                        <td className={`px-3 py-2 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{p.referenceNumber || '-'}</td>
                      </tr>
                    ))}
                    {paymentHistory.length === 0 && (
                      <tr><td colSpan={4} className={`px-3 py-2 text-center ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>No payments yet</td></tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}
      </Modal>

      <Modal isOpen={showPayModal} onClose={() => setShowPayModal(false)} title="Record Payment">
        {supplierDebt && (
          <div className="space-y-4">
            <div className={`p-3 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
              <p className={`text-sm ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Outstanding: <span className="font-bold text-red-500">{formatCurrency(supplierDebt.outstandingDebt)}</span></p>
            </div>
            <div>
              <label className={`text-sm font-medium mb-1 block ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Amount</label>
              <input type="number" value={payAmount} onChange={e => setPayAmount(e.target.value)} className={inputClass} placeholder="Enter amount" />
              {payAmount && !isNaN(parseFloat(payAmount)) && (
                <p className={`text-xs mt-1 ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Remaining: {formatCurrency(Math.max(0, supplierDebt.outstandingDebt - parseFloat(payAmount)))}</p>
              )}
            </div>
            <div>
              <label className={`text-sm font-medium mb-1 block ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Payment Method</label>
              <select value={payMethod} onChange={e => setPayMethod(e.target.value)} className={inputClass}>
                <option value="cash">Cash</option>
                <option value="bank_transfer">Bank Transfer</option>
                <option value="mobile_money">Mobile Money</option>
                <option value="check">Check</option>
              </select>
            </div>
            <div>
              <label className={`text-sm font-medium mb-1 block ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Reference</label>
              <input type="text" value={payReference} onChange={e => setPayReference(e.target.value)} className={inputClass} placeholder="Reference number" />
            </div>
            <div>
              <label className={`text-sm font-medium mb-1 block ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Notes</label>
              <input type="text" value={payNotes} onChange={e => setPayNotes(e.target.value)} className={inputClass} placeholder="Notes" />
            </div>
            {payError && <p className="text-red-500 text-sm">{payError}</p>}
            <div className="flex justify-end gap-2">
              <button onClick={() => setShowPayModal(false)} className={`px-4 py-2 rounded-xl ${isDark ? 'bg-gray-700 text-gray-300' : 'bg-gray-100 text-gray-700'}`}>Cancel</button>
              <button onClick={handlePayDebt} disabled={payLoading} className="px-4 py-2 bg-gradient-to-r from-red-600 to-orange-600 text-white rounded-xl disabled:opacity-50">Record Payment</button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
};
