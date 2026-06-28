import React, { useState, useMemo, useEffect } from 'react';
import { Plus, Search, Edit2, Trash2, Eye, Truck, Phone, Mail, MapPin, User, DollarSign, AlertCircle, CheckCircle } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Modal } from '../components/ui/Modal';
import { Badge } from '../components/ui/Badge';
import { formatDate, generateId, formatCurrency } from '../utils/helpers';
import { Supplier } from '../types';

export const SuppliersPage: React.FC = () => {
  const { suppliers, purchases, fetchSuppliers, fetchPurchases, addSupplier, updateSupplier, deleteSupplier, addAuditLog, currentUser, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    fetchSuppliers();
    fetchPurchases();
  }, [fetchSuppliers, fetchPurchases]);
  const [search, setSearch] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedSupplier, setSelectedSupplier] = useState<Supplier | null>(null);
  const [formData, setFormData] = useState({ 
    name: '', 
    phone: '', 
    email: '', 
    address: '', 
    contactPerson: '',
  });

  // Calculate financial summary for each supplier from purchases
  const supplierFinancials = useMemo(() => {
    const summary: Record<string, SupplierFinancialSummary> = {};
    
    suppliers.forEach(supplier => {
      summary[supplier.id] = {
        supplierId: supplier.id,
        totalPurchases: 0,
        totalPaid: 0,
        totalDebt: 0,
        purchaseCount: 0,
        status: 'cleared',
      };
    });

    purchases.forEach(purchase => {
      if (summary[purchase.supplierId]) {
        summary[purchase.supplierId].totalPurchases += purchase.totalAmount;
        summary[purchase.supplierId].totalPaid += purchase.amountPaid;
        summary[purchase.supplierId].totalDebt += purchase.remainingDebt;
        summary[purchase.supplierId].purchaseCount += 1;
      }
    });

    // Update status based on debt
    Object.values(summary).forEach(s => {
      s.status = s.totalDebt > 0 ? 'outstanding' : 'cleared';
    });

    return summary;
  }, [suppliers, purchases]);

  const filteredSuppliers = useMemo(() => {
    return suppliers.filter(s =>
      s.name.toLowerCase().includes(search.toLowerCase()) ||
      s.phone.includes(search) ||
      s.email.toLowerCase().includes(search.toLowerCase()) ||
      s.contactPerson.toLowerCase().includes(search.toLowerCase())
    );
  }, [suppliers, search]);

  const getSupplierPurchases = (supplierId: string) => purchases.filter(p => p.supplierId === supplierId);

  // Global stats
  const totalDebt = Object.values(supplierFinancials).reduce((sum, s) => sum + s.totalDebt, 0);
  const suppliersWithDebt = Object.values(supplierFinancials).filter(s => s.totalDebt > 0).length;

  const resetForm = () => {
    setFormData({ name: '', phone: '', email: '', address: '', contactPerson: '' });
  };

  const handleAdd = () => {
    resetForm();
    setShowAddModal(true);
  };

  const handleEdit = (supplier: Supplier) => {
    setSelectedSupplier(supplier);
    setFormData({ 
      name: supplier.name, 
      phone: supplier.phone, 
      email: supplier.email, 
      address: supplier.address, 
      contactPerson: supplier.contactPerson,
    });
    setShowEditModal(true);
  };

  const handleView = (supplier: Supplier) => {
    setSelectedSupplier(supplier);
    setShowDetailModal(true);
  };

  const handleSave = () => {
    const newSupplier: Supplier = {
      id: generateId(),
      name: formData.name,
      phone: formData.phone,
      email: formData.email,
      address: formData.address,
      contactPerson: formData.contactPerson,
      isActive: true,
      createdAt: new Date().toISOString(),
    };
    addSupplier(newSupplier);
    addAuditLog({
      id: generateId(),
      userId: currentUser?.id || '',
      userName: currentUser?.fullName || '',
      action: `Added supplier: ${newSupplier.name}`,
      tableName: 'Suppliers',
      recordId: newSupplier.id,
      createdAt: new Date().toISOString(),
    });
    setShowAddModal(false);
    resetForm();
  };

  const handleUpdate = () => {
    if (!selectedSupplier) return;
    updateSupplier(selectedSupplier.id, {
      name: formData.name,
      phone: formData.phone,
      email: formData.email,
      address: formData.address,
      contactPerson: formData.contactPerson,
    });
    addAuditLog({
      id: generateId(),
      userId: currentUser?.id || '',
      userName: currentUser?.fullName || '',
      action: `Updated supplier: ${formData.name}`,
      tableName: 'Suppliers',
      recordId: selectedSupplier.id,
      createdAt: new Date().toISOString(),
    });
    setShowEditModal(false);
    setSelectedSupplier(null);
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this supplier?')) {
      const supplier = suppliers.find(s => s.id === id);
      deleteSupplier(id);
      addAuditLog({
        id: generateId(),
        userId: currentUser?.id || '',
        userName: currentUser?.fullName || '',
        action: `Deleted supplier: ${supplier?.name}`,
        tableName: 'Suppliers',
        recordId: id,
        createdAt: new Date().toISOString(),
      });
    }
  };

  const inputClass = `w-full px-4 py-3 border rounded-xl focus:ring-2 focus:ring-violet-500 focus:border-violet-500 transition-all ${
    isDark 
      ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500 focus:bg-gray-700' 
      : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400'
  }`;
  const thClass = `text-left px-6 py-3 text-xs font-semibold uppercase whitespace-nowrap ${isDark ? 'text-gray-400' : 'text-gray-500'}`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Supplier Management</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Register suppliers • Financial data from purchases</p>
        </div>
        <button
          onClick={handleAdd}
          className="px-4 py-2.5 bg-gradient-to-r from-violet-600 to-purple-600 hover:from-violet-700 hover:to-purple-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2"
        >
          <Plus className="h-4 w-4" /> Add Supplier
        </button>
      </div>

      {/* Info Banner */}
      <div className={`rounded-xl p-4 border ${isDark ? 'bg-blue-900/20 border-blue-800' : 'bg-blue-50 border-blue-200'}`}>
        <div className="flex items-start gap-3">
          <AlertCircle className="h-5 w-5 text-blue-500 mt-0.5 flex-shrink-0" />
          <div>
            <p className={`text-sm font-medium ${isDark ? 'text-blue-300' : 'text-blue-700'}`}>Financial Data from Purchases</p>
            <p className={`text-xs mt-1 ${isDark ? 'text-blue-400' : 'text-blue-600'}`}>
              Supplier financial information (total purchases, paid amount, debt) is automatically calculated from purchase records.
            </p>
          </div>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
              <Truck className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Suppliers</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{suppliers.length}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-green-900/30 text-green-400' : 'bg-green-100 text-green-600'}`}>
              <CheckCircle className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Cleared</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{suppliers.length - suppliersWithDebt}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-600'}`}>
              <AlertCircle className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>With Debt</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{suppliersWithDebt}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-600'}`}>
              <DollarSign className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Debt</p>
              <p className={`text-xl font-bold text-red-500`}>{formatCurrency(totalDebt)}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Search */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
        <div className="relative">
          <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
          <input 
            type="text" 
            placeholder="Search by name, phone, email, or contact person..." 
            value={search} 
            onChange={(e) => setSearch(e.target.value)} 
            className={inputClass}
          />
        </div>
      </div>

      {/* Table */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={thClass}>Supplier</th>
                <th className={thClass}>Contact</th>
                <th className={`${thClass} text-right`}>Purchases</th>
                <th className={`${thClass} text-right`}>Total Spent</th>
                <th className={`${thClass} text-right`}>Paid</th>
                <th className={`${thClass} text-right`}>Debt</th>
                <th className={`${thClass} text-center`}>Status</th>
                <th className={`${thClass} text-right`}>Actions</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {filteredSuppliers.map(supplier => {
                const financial = supplierFinancials[supplier.id];
                return (
                  <tr key={supplier.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                    <td className="px-6 py-4">
                      <div className="flex items-center gap-3">
                        <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-violet-900/30 text-violet-400' : 'bg-violet-100 text-violet-600'}`}>
                          <Truck className="h-5 w-5" />
                        </div>
                        <div>
                          <p className={`font-medium whitespace-nowrap ${isDark ? 'text-white' : 'text-gray-900'}`}>{supplier.name}</p>
                          <p className={`text-xs whitespace-nowrap ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{supplier.address}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-6 py-4">
                      <div className="space-y-1">
                        <div className={`flex items-center gap-1 text-sm whitespace-nowrap ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>
                          <Phone className="h-3 w-3" /> {supplier.phone}
                        </div>
                        <div className={`flex items-center gap-1 text-sm whitespace-nowrap ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
                          <Mail className="h-3 w-3" /> {supplier.email}
                        </div>
                      </div>
                    </td>
                    <td className={`px-6 py-4 text-right text-sm font-medium whitespace-nowrap ${isDark ? 'text-white' : 'text-gray-900'}`}>
                      {financial?.purchaseCount || 0}
                    </td>
                    <td className={`px-6 py-4 text-right text-sm font-medium whitespace-nowrap ${isDark ? 'text-white' : 'text-gray-900'}`}>
                      {formatCurrency(financial?.totalPurchases || 0)}
                    </td>
                    <td className={`px-6 py-4 text-right text-sm whitespace-nowrap ${isDark ? 'text-green-400' : 'text-green-600'}`}>
                      {formatCurrency(financial?.totalPaid || 0)}
                    </td>
                    <td className="px-6 py-4 text-right">
                      {financial && financial.totalDebt > 0 ? (
                        <span className="text-sm font-semibold text-red-500 whitespace-nowrap">{formatCurrency(financial.totalDebt)}</span>
                      ) : (
                        <span className={`text-sm whitespace-nowrap ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>-</span>
                      )}
                    </td>
                    <td className="px-6 py-4 text-center">
                      <Badge variant={financial?.status === 'cleared' ? 'success' : 'warning'}>
                        {financial?.status === 'cleared' ? '✅ Cleared' : '⚠️ Outstanding'}
                      </Badge>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex items-center justify-end gap-1">
                        <button onClick={() => handleView(supplier)} className={`p-1.5 rounded-lg transition-colors ${isDark ? 'hover:bg-blue-900/30 text-gray-400 hover:text-blue-400' : 'hover:bg-blue-50 text-gray-400 hover:text-blue-600'}`}>
                          <Eye className="h-4 w-4" />
                        </button>
                        <button onClick={() => handleEdit(supplier)} className={`p-1.5 rounded-lg transition-colors ${isDark ? 'hover:bg-amber-900/30 text-gray-400 hover:text-amber-400' : 'hover:bg-amber-50 text-gray-400 hover:text-amber-600'}`}>
                          <Edit2 className="h-4 w-4" />
                        </button>
                        <button onClick={() => handleDelete(supplier.id)} className={`p-1.5 rounded-lg transition-colors ${isDark ? 'hover:bg-red-900/30 text-gray-400 hover:text-red-400' : 'hover:bg-red-50 text-gray-400 hover:text-red-600'}`}>
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>

      {/* Add Modal */}
      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Add New Supplier">
        <div className="space-y-5">
          <div className="flex items-center gap-4 pb-2">
            <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${isDark ? 'bg-violet-900/30 text-violet-400' : 'bg-violet-100 text-violet-600'}`}>
              <Truck className="h-7 w-7" />
            </div>
            <div>
              <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>New Supplier</p>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Register a new business partner</p>
            </div>
          </div>

          <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
            <h4 className={`text-sm font-semibold mb-4 ${isDark ? 'text-gray-200' : 'text-gray-700'}`}>Supplier Information</h4>
            <div className="space-y-4">
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  <Truck className="h-4 w-4" /> Supplier Name <span className="text-red-500">*</span>
                </label>
                <input type="text" value={formData.name} onChange={e => setFormData({ ...formData, name: e.target.value })} placeholder="Enter supplier name" className={inputClass} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    <Phone className="h-4 w-4" /> Phone <span className="text-red-500">*</span>
                  </label>
                  <input type="text" value={formData.phone} onChange={e => setFormData({ ...formData, phone: e.target.value })} placeholder="+251..." className={inputClass} />
                </div>
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    <Mail className="h-4 w-4" /> Email
                  </label>
                  <input type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} placeholder="email@example.com" className={inputClass} />
                </div>
              </div>
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  <MapPin className="h-4 w-4" /> Address
                </label>
                <input type="text" value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} placeholder="City, Street..." className={inputClass} />
              </div>
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  <User className="h-4 w-4" /> Contact Person
                </label>
                <input type="text" value={formData.contactPerson} onChange={e => setFormData({ ...formData, contactPerson: e.target.value })} placeholder="Contact person name" className={inputClass} />
              </div>
            </div>
          </div>

          <div className={`rounded-lg p-4 ${isDark ? 'bg-blue-900/20 border border-blue-800' : 'bg-blue-50 border border-blue-200'}`}>
            <p className={`text-sm ${isDark ? 'text-blue-300' : 'text-blue-700'}`}>
              💡 Payment information is managed in the Purchase module when creating purchase orders.
            </p>
          </div>

          <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <button onClick={() => setShowAddModal(false)} className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>
              Cancel
            </button>
            <button onClick={handleSave} className="px-5 py-2.5 bg-gradient-to-r from-violet-600 to-purple-600 hover:from-violet-700 hover:to-purple-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2">
              <Truck className="h-4 w-4" /> Save Supplier
            </button>
          </div>
        </div>
      </Modal>

      {/* Edit Modal */}
      <Modal isOpen={showEditModal} onClose={() => setShowEditModal(false)} title="Edit Supplier">
        {selectedSupplier && (
          <div className="space-y-5">
            <div className="flex items-center gap-4 pb-2">
              <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                <Truck className="h-7 w-7" />
              </div>
              <div>
                <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedSupplier.name}</p>
                <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Update supplier information</p>
              </div>
            </div>

            <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
              <div className="space-y-4">
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    <Truck className="h-4 w-4" /> Supplier Name
                  </label>
                  <input type="text" value={formData.name} onChange={e => setFormData({ ...formData, name: e.target.value })} className={inputClass} />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                      <Phone className="h-4 w-4" /> Phone
                    </label>
                    <input type="text" value={formData.phone} onChange={e => setFormData({ ...formData, phone: e.target.value })} className={inputClass} />
                  </div>
                  <div>
                    <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                      <Mail className="h-4 w-4" /> Email
                    </label>
                    <input type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} className={inputClass} />
                  </div>
                </div>
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    <MapPin className="h-4 w-4" /> Address
                  </label>
                  <input type="text" value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} className={inputClass} />
                </div>
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    <User className="h-4 w-4" /> Contact Person
                  </label>
                  <input type="text" value={formData.contactPerson} onChange={e => setFormData({ ...formData, contactPerson: e.target.value })} className={inputClass} />
                </div>
              </div>
            </div>

            <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
              <button onClick={() => setShowEditModal(false)} className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>
                Cancel
              </button>
              <button onClick={handleUpdate} className="px-5 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2">
                <Truck className="h-4 w-4" /> Update Supplier
              </button>
            </div>
          </div>
        )}
      </Modal>

      {/* Detail Modal */}
      <Modal isOpen={showDetailModal} onClose={() => setShowDetailModal(false)} title="Supplier Details" size="lg">
        {selectedSupplier && (
          <div className="space-y-6">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Name</p>
                <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedSupplier.name}</p>
              </div>
              <div>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Contact Person</p>
                <p className={isDark ? 'text-gray-300' : 'text-gray-900'}>{selectedSupplier.contactPerson || '-'}</p>
              </div>
              <div>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Phone</p>
                <p className={isDark ? 'text-gray-300' : 'text-gray-900'}>{selectedSupplier.phone}</p>
              </div>
              <div>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Email</p>
                <p className={isDark ? 'text-gray-300' : 'text-gray-900'}>{selectedSupplier.email}</p>
              </div>
              <div className="col-span-2">
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Address</p>
                <p className={isDark ? 'text-gray-300' : 'text-gray-900'}>{selectedSupplier.address}</p>
              </div>
            </div>

            {/* Financial Summary from Purchases */}
            {(() => {
              const financial = supplierFinancials[selectedSupplier.id];
              return (
                <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
                  <h4 className={`text-sm font-semibold mb-4 ${isDark ? 'text-gray-200' : 'text-gray-700'}`}>Financial Summary (From Purchases)</h4>
                  <div className="grid grid-cols-4 gap-3">
                    <div className={`p-4 rounded-xl text-center ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                      <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Purchases</p>
                      <p className={`text-xl font-bold mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{financial?.purchaseCount || 0}</p>
                    </div>
                    <div className={`p-4 rounded-xl text-center ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                      <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Spent</p>
                      <p className={`text-xl font-bold mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{formatCurrency(financial?.totalPurchases || 0)}</p>
                    </div>
                    <div className={`p-4 rounded-xl text-center ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                      <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Paid</p>
                      <p className="text-xl font-bold text-green-500 mt-1">{formatCurrency(financial?.totalPaid || 0)}</p>
                    </div>
                    <div className={`p-4 rounded-xl text-center ${isDark ? 'bg-gray-800' : 'bg-white'}`}>
                      <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Debt</p>
                      <p className={`text-xl font-bold mt-1 ${(financial?.totalDebt || 0) > 0 ? 'text-red-500' : 'text-green-500'}`}>
                        {formatCurrency(financial?.totalDebt || 0)}
                      </p>
                    </div>
                  </div>
                </div>
              );
            })()}

            {/* Purchase History */}
            <div>
              <h4 className={`text-sm font-semibold mb-3 ${isDark ? 'text-white' : 'text-gray-900'}`}>Purchase History</h4>
              <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
                <table className="w-full text-sm">
                  <thead>
                    <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                      <th className={`text-left px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Purchase #</th>
                      <th className={`text-left px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Date</th>
                      <th className={`text-right px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total</th>
                      <th className={`text-right px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Paid</th>
                      <th className={`text-right px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Debt</th>
                      <th className={`text-center px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</th>
                    </tr>
                  </thead>
                  <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                    {getSupplierPurchases(selectedSupplier.id).map(p => (
                      <tr key={p.id}>
                        <td className={`px-4 py-2 font-mono text-blue-500`}>{p.purchaseNumber}</td>
                        <td className={`px-4 py-2 ${isDark ? 'text-gray-300' : ''}`}>{formatDate(p.purchaseDate)}</td>
                        <td className={`px-4 py-2 text-right ${isDark ? 'text-white' : ''}`}>{formatCurrency(p.totalAmount)}</td>
                        <td className={`px-4 py-2 text-right ${isDark ? 'text-green-400' : 'text-green-600'}`}>{formatCurrency(p.amountPaid)}</td>
                        <td className={`px-4 py-2 text-right ${p.remainingDebt > 0 ? 'text-red-500' : isDark ? 'text-gray-400' : 'text-gray-500'}`}>
                          {p.remainingDebt > 0 ? formatCurrency(p.remainingDebt) : '-'}
                        </td>
                        <td className="px-4 py-2 text-center">
                          <Badge variant={p.paymentStatus === 'paid' ? 'success' : p.paymentStatus === 'partial' ? 'warning' : 'danger'}>
                            {p.paymentStatus === 'paid' ? 'Paid' : p.paymentStatus === 'partial' ? 'Partial' : 'Unpaid'}
                          </Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};
