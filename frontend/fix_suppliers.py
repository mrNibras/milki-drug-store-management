import sys

path = '/home/mrnibra/milki-drug-store-management/frontend/src/pages/SuppliersPage.tsx'
with open(path, 'r') as f:
    content = f.read()

# Find the position where we need to insert - look for Detail Modal end
# The file was truncated, need to reconstruct properly
# Find "Detail Modal" section and append modals before final closing
idx = content.rfind('      </Modal>')
if idx == -1:
    print('not found')
    sys.exit()

# Find the end of that modal
# Look for the next occurrence of </div> followed by </Modal>
# Actually easier: find "Detail Modal" and work backwards
# Just append modals properly
modals = '''
      {/* Debt Modal */}
      <Modal isOpen={showDebtModal} onClose={() => setShowDebtModal(false)} title="Supplier Debt & Payments" size="lg">
        {selectedDebtSupplier && (
          <div className="space-y-4">
            <div className={`p-4 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
              <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedDebtSupplier.name}</p>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{selectedDebtSupplier.phone}</p>
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

      {/* Pay Debt Modal */}
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
    </div>
  );
};
'''

# Remove everything from the broken modals to end and add properly
# Find the Detail Modal end
detail_end = content.rfind('      </Modal>')
# Find the start of the next content after Detail Modal
# Look for the pattern
idx = content.find('      </Modal>', detail_end + 1)
if idx == -1:
    # Just append
    content = content[:content.rfind('      </Modal>') + len('      </Modal>')] + '\n' + modals
else:
    content = content[:content.rfind('      </Modal>') + len('      </Modal>')] + '\n' + modals

with open(path, 'w') as f:
    f.write(content)
print('done')
