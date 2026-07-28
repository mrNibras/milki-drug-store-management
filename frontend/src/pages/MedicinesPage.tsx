import React, { useState, useMemo, useEffect } from 'react';
import { Plus, Search, Edit2, Trash2, Eye, Package, Pill, Layers, Info } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Modal } from '../components/ui/Modal';
import { Badge } from '../components/ui/Badge';
import { formatDate, getExpiryStatus, getExpiryColor, getStockStatus, getStockColor, generateId } from '../utils/helpers';
import { Medicine } from '../types';

export const MedicinesPage: React.FC = () => {
  const { medicines, categories, unitTypes, fetchMedicines, fetchCategories, fetchUnitTypes, addMedicine, updateMedicine, deleteMedicine, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [search, setSearch] = useState('');
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDetailModal, setShowDetailModal] = useState(false);
  const [selectedMedicine, setSelectedMedicine] = useState<Medicine | null>(null);
  const [formData, setFormData] = useState({
    name: '', genericName: '', categoryId: '', unitTypeId: 1, lowStockThreshold: 10,
  });

  useEffect(() => {
    fetchMedicines();
    fetchCategories();
    fetchUnitTypes();
  }, [fetchMedicines, fetchCategories, fetchUnitTypes]);

  const filteredMedicines = useMemo(() => {
    return (medicines || []).filter(m => {
      const matchSearch = m.name?.toLowerCase().includes(search.toLowerCase()) ||
        m.genericName?.toLowerCase().includes(search.toLowerCase()) ||
        m.categoryName?.toLowerCase().includes(search.toLowerCase());
      const matchCategory = categoryFilter === 'all' || m.categoryId === categoryFilter;
      const totalQty = (m.batches || []).reduce((sum, b) => sum + (b.quantity || 0), 0);
      const status = getStockStatus(totalQty, m.lowStockThreshold);
      const matchStatus = statusFilter === 'all' || status === statusFilter;
      return matchSearch && matchCategory && matchStatus;
    }).sort((a, b) => a.name.localeCompare(b.name));
  }, [medicines, search, categoryFilter, statusFilter]);

  const getMedicineStock = (m: Medicine) => (m.batches || []).reduce((sum, b) => sum + (b.quantity || 0), 0);

  const handleAdd = () => {
    setFormData({ name: '', genericName: '', categoryId: '', unitTypeId: 1, lowStockThreshold: 10 });
    setShowAddModal(true);
  };

  const handleEdit = (medicine: Medicine) => {
    setSelectedMedicine(medicine);
    setFormData({
      name: medicine.name,
      genericName: medicine.genericName,
      categoryId: medicine.categoryId,
      unitTypeId: medicine.unitTypeId,
      lowStockThreshold: medicine.lowStockThreshold,
    });
    setShowEditModal(true);
  };

  const handleView = (medicine: Medicine) => {
    setSelectedMedicine(medicine);
    setShowDetailModal(true);
  };

  const handleSave = () => {
    const newMedicine: Medicine = {
      id: generateId(),
      name: formData.name,
      genericName: formData.genericName,
      categoryId: formData.categoryId,
      categoryName: (categories || []).find(c => c.id === formData.categoryId)?.name || '',
      unitType: (unitTypes || []).find(u => u.id === String(formData.unitTypeId))?.name || 'Tablet',
      unitTypeId: formData.unitTypeId,
      lowStockThreshold: formData.lowStockThreshold,
      createdAt: new Date().toISOString(),
      batches: [],
    };
    addMedicine(newMedicine);
    setShowAddModal(false);
  };

  const handleUpdate = () => {
    if (!selectedMedicine) return;
    updateMedicine(selectedMedicine.id, {
      name: formData.name,
      genericName: formData.genericName,
      categoryId: formData.categoryId,
      categoryName: categories.find(c => c.id === formData.categoryId)?.name || '',
      unitTypeId: formData.unitTypeId,
      lowStockThreshold: formData.lowStockThreshold,
      isActive: selectedMedicine.isActive,
    });
    setShowEditModal(false);
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to delete this medicine?')) {
      deleteMedicine(id);
    }
  };

  const categoryList = useMemo(() => {
    const seen = new Set<string>();
    return medicines.reduce<{ id: string; name: string }[]>((acc, m) => {
      if (!seen.has(m.categoryId)) {
        seen.add(m.categoryId);
        acc.push({ id: m.categoryId, name: m.categoryName });
      }
      return acc;
    }, []);
  }, [medicines]);

  const inputClass = `w-full px-4 py-3 border rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all ${
    isDark 
      ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500 focus:bg-gray-700' 
      : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400'
  }`;
  const thClass = `text-left px-5 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Medicine Management</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Register and manage medicine catalog</p>
        </div>
        <button
          onClick={handleAdd}
          className="px-4 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg shadow-blue-200 dark:shadow-blue-900/30 flex items-center gap-2"
        >
          <Plus className="h-4 w-4" /> Add Medicine
        </button>
      </div>

      {/* Info Banner */}
      <div className={`rounded-xl p-4 border ${isDark ? 'bg-blue-900/20 border-blue-800' : 'bg-blue-50 border-blue-200'}`}>
        <div className="flex items-start gap-3">
          <Info className="h-5 w-5 text-blue-500 mt-0.5 flex-shrink-0" />
          <div>
            <p className={`text-sm font-medium ${isDark ? 'text-blue-300' : 'text-blue-700'}`}>Medicine Registration</p>
            <p className={`text-xs mt-1 ${isDark ? 'text-blue-400' : 'text-blue-600'}`}>
              This page is for registering medicines only. Stock, batches, prices, and expiry dates are managed in the <strong>Purchase Module</strong>.
            </p>
          </div>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
              <Pill className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Medicines</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{medicines.length}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-violet-900/30 text-violet-400' : 'bg-violet-100 text-violet-600'}`}>
              <Layers className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Categories</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{categoryList.length}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-600'}`}>
              <Package className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>With Stock</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{(medicines || []).filter(m => (m.batches || []).length > 0).length}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-gray-700 text-gray-400' : 'bg-gray-100 text-gray-600'}`}>
              <Package className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>No Stock</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{(medicines || []).filter(m => (m.batches || []).length === 0).length}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
        <div className="flex flex-col md:flex-row gap-3">
          <div className="relative flex-1">
            <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
            <input
              type="text"
              placeholder="Search by medicine name, generic name, or category..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className={`w-full pl-9 pr-4 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-sm ${
                isDark ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-400' : 'bg-white border-gray-300 text-gray-900'
              }`}
            />
          </div>
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className={`px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm ${
              isDark ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-900'
            }`}
          >
            <option value="all">All Categories</option>
            {categoryList.map(cat => (
              <option key={cat.id} value={cat.id}>{cat.name}</option>
            ))}
          </select>
          <select
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            className={`px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm ${
              isDark ? 'bg-gray-700 border-gray-600 text-white' : 'bg-white border-gray-300 text-gray-900'
            }`}
          >
            <option value="all">All Status</option>
            <option value="ok">In Stock</option>
            <option value="low">Low Stock</option>
            <option value="out_of_stock">Out of Stock</option>
          </select>
        </div>
      </div>

      {/* Table */}
      {loading ? (
        <div className="text-center py-12">
          <div className="h-8 w-8 border-2 border-emerald-500 border-t-transparent rounded-full animate-spin mx-auto mb-3" />
          <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>Loading medicines...</p>
        </div>
      ) : (
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={thClass}>Medicine</th>
                <th className={thClass}>Category</th>
                <th className={thClass}>Unit Type</th>
                <th className={thClass}>Batches</th>
                <th className={thClass}>Total Stock</th>
                <th className={thClass}>Status</th>
                <th className={`${thClass} text-right`}>Actions</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {filteredMedicines.map((medicine) => {
                const totalStock = getMedicineStock(medicine);
                const stockStatus = getStockStatus(totalStock, medicine.lowStockThreshold);
                const batchCount = medicine.batches.length;

                return (
                  <tr key={medicine.id} className={`transition-colors ${isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}`}>
                    <td className="px-5 py-4">
                      <div className="flex items-center gap-3">
                        <div className={`flex h-10 w-10 items-center justify-center rounded-lg flex-shrink-0 ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                          <Pill className="h-5 w-5" />
                        </div>
                        <div>
                          <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{medicine.name}</p>
                          <p className={`text-xs ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{medicine.genericName}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-5 py-4">
                      <Badge variant="info">{medicine.categoryName}</Badge>
                    </td>
                    <td className={`px-5 py-4 text-sm ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                      {medicine.unitType}
                    </td>
                    <td className="px-5 py-4">
                      <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-medium ${
                        batchCount > 0 
                          ? isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-700'
                          : isDark ? 'bg-gray-700 text-gray-400' : 'bg-gray-100 text-gray-500'
                      }`}>
                        <Layers className="h-3 w-3" /> {batchCount}
                      </span>
                    </td>
                    <td className="px-5 py-4">
                      <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ${getStockColor(stockStatus)}`}>
                        {totalStock} {medicine.unitType}{totalStock !== 1 ? 's' : ''}
                      </span>
                    </td>
                    <td className="px-5 py-4">
                      {totalStock === 0 ? (
                        <Badge variant="danger">No Stock</Badge>
                      ) : stockStatus === 'low' ? (
                        <Badge variant="warning">Low Stock</Badge>
                      ) : (
                        <Badge variant="success">In Stock</Badge>
                      )}
                    </td>
                    <td className="px-5 py-4">
                      <div className="flex items-center justify-end gap-1">
                        <button
                          onClick={() => handleView(medicine)}
                          className={`p-1.5 rounded-lg transition-colors ${isDark ? 'hover:bg-blue-900/30 text-gray-400 hover:text-blue-400' : 'hover:bg-blue-50 text-gray-400 hover:text-blue-600'}`}
                          title="View Details"
                        >
                          <Eye className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => handleEdit(medicine)}
                          className={`p-1.5 rounded-lg transition-colors ${isDark ? 'hover:bg-amber-900/30 text-gray-400 hover:text-amber-400' : 'hover:bg-amber-50 text-gray-400 hover:text-amber-600'}`}
                          title="Edit"
                        >
                          <Edit2 className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(medicine.id)}
                          className={`p-1.5 rounded-lg transition-colors ${isDark ? 'hover:bg-red-900/30 text-gray-400 hover:text-red-400' : 'hover:bg-red-50 text-gray-400 hover:text-red-600'}`}
                          title="Delete"
                        >
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
        {filteredMedicines.length === 0 && (
          <div className="text-center py-12">
            <Package className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
            <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No medicines found</p>
          </div>
        )}
      </div>
      )}

      {/* ========== ADD MEDICINE MODAL ========== */}
      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Register New Medicine">
        <div className="space-y-5">
          <div className="flex items-center gap-4 pb-2">
            <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
              <Pill className="h-7 w-7" />
            </div>
            <div>
              <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>New Medicine</p>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Add medicine to the catalog</p>
            </div>
          </div>

          <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
            <h4 className={`text-sm font-semibold mb-4 ${isDark ? 'text-gray-200' : 'text-gray-700'}`}>Medicine Information</h4>
            <div className="space-y-4">
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  <Pill className="h-4 w-4" /> Medicine Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.name}
                  onChange={e => setFormData({ ...formData, name: e.target.value })}
                  placeholder="e.g., Paracetamol 500mg"
                  className={inputClass}
                />
              </div>
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  Generic Name
                </label>
                <input
                  type="text"
                  value={formData.genericName}
                  onChange={e => setFormData({ ...formData, genericName: e.target.value })}
                  placeholder="e.g., Acetaminophen"
                  className={inputClass}
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    Category <span className="text-red-500">*</span>
                  </label>
                  <select
                    value={formData.categoryId}
                    onChange={e => setFormData({ ...formData, categoryId: e.target.value })}
                    className={inputClass}
                  >
                    <option value="">Select Category</option>
                    {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    Unit Type
                  </label>
                  <select
                    value={String(formData.unitTypeId)}
                    onChange={e => setFormData({ ...formData, unitTypeId: Number(e.target.value) })}
                    className={inputClass}
                  >
                    {unitTypes.map(u => <option key={u.id} value={Number(u.id)}>{u.name}</option>)}
                  </select>
                </div>
              </div>
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  Low Stock Threshold
                </label>
                <input
                  type="number"
                  value={formData.lowStockThreshold}
                  onChange={e => setFormData({ ...formData, lowStockThreshold: Number(e.target.value) })}
                  placeholder="10"
                  className={inputClass}
                />
                <p className={`text-xs mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>Alert when stock falls below this number</p>
              </div>
            </div>
          </div>

          <div className={`rounded-lg p-4 ${isDark ? 'bg-amber-900/20 border border-amber-800' : 'bg-amber-50 border border-amber-200'}`}>
            <p className={`text-sm ${isDark ? 'text-amber-300' : 'text-amber-700'}`}>
              💡 <strong>Note:</strong> After registering the medicine, use the <strong>Purchase Module</strong> to add stock, set prices, batch numbers, and expiry dates.
            </p>
          </div>

          <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <button
              onClick={() => setShowAddModal(false)}
              className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}
            >
              Cancel
            </button>
            <button
              onClick={handleSave}
              className="px-5 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2"
            >
              <Pill className="h-4 w-4" /> Register Medicine
            </button>
          </div>
        </div>
      </Modal>

      {/* ========== EDIT MEDICINE MODAL ========== */}
      <Modal isOpen={showEditModal} onClose={() => setShowEditModal(false)} title="Edit Medicine">
        {selectedMedicine && (
          <div className="space-y-5">
            <div className="flex items-center gap-4 pb-2">
              <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${isDark ? 'bg-amber-900/30 text-amber-400' : 'bg-amber-100 text-amber-600'}`}>
                <Pill className="h-7 w-7" />
              </div>
              <div>
                <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedMedicine.name}</p>
                <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Update medicine information</p>
              </div>
            </div>

            <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
              <h4 className={`text-sm font-semibold mb-4 ${isDark ? 'text-gray-200' : 'text-gray-700'}`}>Medicine Information</h4>
              <div className="space-y-4">
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    <Pill className="h-4 w-4" /> Medicine Name
                  </label>
                  <input
                    type="text"
                    value={formData.name}
                    onChange={e => setFormData({ ...formData, name: e.target.value })}
                    className={inputClass}
                  />
                </div>
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    Generic Name
                  </label>
                  <input
                    type="text"
                    value={formData.genericName}
                    onChange={e => setFormData({ ...formData, genericName: e.target.value })}
                    className={inputClass}
                  />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                      Category
                    </label>
                    <select
                      value={formData.categoryId}
                      onChange={e => setFormData({ ...formData, categoryId: e.target.value })}
                      className={inputClass}
                    >
                      {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                    </select>
                  </div>
                  <div>
                    <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                      Unit Type
                    </label>
                    <select
                      value={String(formData.unitTypeId)}
                      onChange={e => setFormData({ ...formData, unitTypeId: Number(e.target.value) })}
                      className={inputClass}
                    >
                      {unitTypes.map(u => <option key={u.id} value={Number(u.id)}>{u.name}</option>)}
                    </select>
                  </div>
                </div>
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    Low Stock Threshold
                  </label>
                  <input
                    type="number"
                    value={formData.lowStockThreshold}
                    onChange={e => setFormData({ ...formData, lowStockThreshold: Number(e.target.value) })}
                    className={inputClass}
                  />
                </div>
              </div>
            </div>

            <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
              <button
                onClick={() => setShowEditModal(false)}
                className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}
              >
                Cancel
              </button>
              <button
                onClick={handleUpdate}
                className="px-5 py-2.5 bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2"
              >
                <Edit2 className="h-4 w-4" /> Update Medicine
              </button>
            </div>
          </div>
        )}
      </Modal>

      {/* ========== DETAIL MODAL ========== */}
      <Modal isOpen={showDetailModal} onClose={() => setShowDetailModal(false)} title="Medicine Details" size="lg">
        {selectedMedicine && (
          <div className="space-y-6">
            <div className="flex items-center gap-4 pb-2">
              <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                <Pill className="h-7 w-7" />
              </div>
              <div>
                <p className={`font-semibold text-lg ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedMedicine.name}</p>
                <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{selectedMedicine.genericName}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className={`p-4 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Category</p>
                <Badge variant="info">{selectedMedicine.categoryName}</Badge>
              </div>
              <div className={`p-4 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Unit Type</p>
                <p className={`font-medium mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedMedicine.unitType}</p>
              </div>
              <div className={`p-4 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Batches</p>
                <p className={`font-medium mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedMedicine.batches.length}</p>
              </div>
              <div className={`p-4 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
                <p className={`text-xs uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Stock</p>
                <p className={`font-medium mt-1 ${isDark ? 'text-white' : 'text-gray-900'}`}>
                  {getMedicineStock(selectedMedicine)} {selectedMedicine.unitType}s
                </p>
              </div>
            </div>

            {/* Batches */}
            {selectedMedicine.batches.length > 0 ? (
              <div>
                <h4 className={`text-sm font-semibold mb-3 flex items-center gap-2 ${isDark ? 'text-white' : 'text-gray-900'}`}>
                  <Layers className="h-4 w-4" /> Batches (FEFO Ordered)
                </h4>
                <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
                  <table className="w-full text-sm">
                    <thead>
                      <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                        <th className={`text-left px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Batch #</th>
                        <th className={`text-left px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Qty</th>
                        <th className={`text-left px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Buy Price</th>
                        <th className={`text-left px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Sell Price</th>
                        <th className={`text-left px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Expiry</th>
                        <th className={`text-left px-4 py-2 text-xs font-medium ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</th>
                      </tr>
                    </thead>
                    <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
                      {[...selectedMedicine.batches]
                        .sort((a, b) => new Date(a.expiryDate).getTime() - new Date(b.expiryDate).getTime())
                        .map(batch => {
                          const expStatus = getExpiryStatus(batch.expiryDate);
                          return (
                            <tr key={batch.id}>
                              <td className={`px-4 py-2 font-mono font-medium ${isDark ? 'text-white' : ''}`}>{batch.batchNumber}</td>
                              <td className={`px-4 py-2 ${isDark ? 'text-gray-300' : ''}`}>{batch.quantity}</td>
                              <td className={`px-4 py-2 ${isDark ? 'text-gray-300' : ''}`}>{batch.purchasePrice.toLocaleString()} ETB</td>
                              <td className={`px-4 py-2 ${isDark ? 'text-gray-300' : ''}`}>{batch.sellingPrice.toLocaleString()} ETB</td>
                              <td className={`px-4 py-2 ${isDark ? 'text-gray-300' : ''}`}>{formatDate(batch.expiryDate)}</td>
                              <td className="px-4 py-2">
                                <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${getExpiryColor(expStatus)}`}>
                                  {expStatus === 'expired' ? 'Expired' : expStatus === 'critical' ? 'Critical' : expStatus === 'warning' ? 'Warning' : 'OK'}
                                </span>
                              </td>
                            </tr>
                          );
                        })}
                    </tbody>
                  </table>
                </div>
              </div>
            ) : (
              <div className={`p-6 rounded-xl border text-center ${isDark ? 'bg-gray-700/50 border-gray-600' : 'bg-gray-50 border-gray-200'}`}>
                <Package className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
                <p className={`font-medium ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>No Stock Yet</p>
                <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>
                  Use the Purchase Module to add stock for this medicine
                </p>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
};
