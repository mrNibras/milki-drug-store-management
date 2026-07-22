import React, { useState, useEffect } from 'react';
import { Plus, Edit2, Trash2, Search, Package, Hash } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Modal } from '../components/ui/Modal';
import { Badge } from '../components/ui/Badge';
import { Cosmetic } from '../types';

export const CosmeticsPage: React.FC = () => {
  const { cosmetics, cosmeticCategories, fetchCosmetics, fetchCosmeticCategories, addCosmetic, updateCosmetic, deleteCosmetic, addCosmeticBatch, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showBatchModal, setShowBatchModal] = useState(false);
  const [selectedCosmetic, setSelectedCosmetic] = useState<Cosmetic | null>(null);
  const [search, setSearch] = useState('');
  const [formData, setFormData] = useState({
    productName: '',
    description: '',
    cosmeticCategoryId: '',
    unitTypeId: 0,
    price: 0,
    isActive: true,
  });
  const [batchData, setBatchData] = useState({
    batchNumber: '',
    quantity: 0,
    purchasePrice: 0,
    sellingPrice: 0,
    expiryDate: '',
  });

  useEffect(() => {
    fetchCosmetics();
    fetchCosmeticCategories();
  }, [fetchCosmetics, fetchCosmeticCategories]);

  const filteredCosmetics = cosmetics.filter(c =>
    c.productName.toLowerCase().includes(search.toLowerCase()) ||
    c.description.toLowerCase().includes(search.toLowerCase())
  );

  const handleAdd = () => {
    setFormData({ productName: '', description: '', cosmeticCategoryId: cosmeticCategories[0]?.id || '', unitTypeId: 0, price: 0, isActive: true });
    setShowAddModal(true);
  };

  const handleEdit = (cosmetic: Cosmetic) => {
    setSelectedCosmetic(cosmetic);
    setFormData({
      productName: cosmetic.productName,
      description: cosmetic.description,
      cosmeticCategoryId: cosmetic.cosmeticCategoryId,
      unitTypeId: cosmetic.unitTypeId,
      price: cosmetic.price,
      isActive: cosmetic.isActive,
    });
    setShowEditModal(true);
  };

  const handleSave = async () => {
    if (!formData.productName.trim()) return;
    await addCosmetic({
      id: '',
      productName: formData.productName,
      description: formData.description,
      cosmeticCategoryId: formData.cosmeticCategoryId,
      cosmeticCategoryName: '',
      unitType: '',
      unitTypeId: formData.unitTypeId,
      price: formData.price,
      isActive: formData.isActive,
      createdAt: new Date().toISOString(),
      batches: [],
    });
    setShowAddModal(false);
  };

  const handleUpdate = async () => {
    if (!selectedCosmetic || !formData.productName.trim()) return;
    await updateCosmetic(selectedCosmetic.id, {
      ...formData,
    });
    setShowEditModal(false);
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to deactivate this cosmetic product?')) {
      deleteCosmetic(id);
    }
  };

  const handleAddBatch = (cosmetic: Cosmetic) => {
    setSelectedCosmetic(cosmetic);
    setBatchData({ batchNumber: '', quantity: 0, purchasePrice: 0, sellingPrice: 0, expiryDate: '' });
    setShowBatchModal(true);
  };

  const handleSaveBatch = async () => {
    if (!selectedCosmetic || !batchData.batchNumber.trim()) return;
    await addCosmeticBatch({
      cosmeticId: Number(selectedCosmetic.id),
      batchNumber: batchData.batchNumber,
      quantity: batchData.quantity,
      purchasePrice: batchData.purchasePrice,
      sellingPrice: batchData.sellingPrice,
      expiryDate: batchData.expiryDate || new Date().toISOString().split('T')[0],
    });
    setShowBatchModal(false);
  };

  const inputClass = `w-full px-4 py-3 border rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all ${
    isDark ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500' : 'bg-white border-gray-300 text-gray-900'
  }`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Cosmetic Inventory</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Manage cosmetic products and stock</p>
        </div>
        <button
          onClick={handleAdd}
          className="px-4 py-2.5 bg-gradient-to-r from-pink-600 to-rose-600 hover:from-pink-700 hover:to-rose-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2"
        >
          <Plus className="h-4 w-4" /> Add Cosmetic
        </button>
      </div>

      <div className="relative">
        <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search cosmetics..."
          className={`w-full pl-10 pr-4 py-3 border rounded-xl focus:ring-2 focus:ring-pink-500 focus:border-pink-500 transition-all ${isDark ? 'bg-gray-800 border-gray-700 text-white placeholder-gray-500' : 'border-gray-300 text-gray-900'}`}
        />
      </div>

      {loading ? (
        <div className="text-center py-12">
          <div className="h-8 w-8 border-2 border-pink-500 border-t-transparent rounded-full animate-spin mx-auto mb-3" />
          <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>Loading cosmetics...</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredCosmetics.map((cosmetic) => (
            <div key={cosmetic.id} className={`rounded-xl border shadow-sm p-5 ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-3">
                  <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-pink-900/30 text-pink-400' : 'bg-pink-100 text-pink-600'}`}>
                    <Package className="h-5 w-5" />
                  </div>
                  <div>
                    <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{cosmetic.productName}</h3>
                    <Badge variant={cosmetic.isActive ? 'success' : 'danger'}>{cosmetic.isActive ? 'Active' : 'Inactive'}</Badge>
                  </div>
                </div>
                <div className="flex gap-1">
                  <button onClick={() => handleEdit(cosmetic)} className={`p-2 rounded-lg transition-colors ${isDark ? 'hover:bg-amber-900/30 text-gray-400 hover:text-amber-400' : 'hover:bg-amber-50 text-gray-400 hover:text-amber-600'}`}>
                    <Edit2 className="h-4 w-4" />
                  </button>
                  <button onClick={() => handleDelete(cosmetic.id)} className={`p-2 rounded-lg transition-colors ${isDark ? 'hover:bg-red-900/30 text-gray-400 hover:text-red-400' : 'hover:bg-red-50 text-gray-400 hover:text-red-600'}`}>
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
              <div className={`space-y-2 text-sm ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                <p>{cosmetic.description}</p>
                <div className="flex items-center justify-between">
                  <span className="font-medium">Price:</span>
                  <span className={isDark ? 'text-white' : 'text-gray-900'}>ETB {cosmetic.price.toFixed(2)}</span>
                </div>
                {cosmetic.batches.length > 0 && (
                  <div className="mt-2 pt-2 border-t border-gray-200 dark:border-gray-700">
                    <p className="text-xs font-medium mb-1">Batches:</p>
                    {cosmetic.batches.slice(0, 2).map(batch => (
                      <div key={batch.id} className="flex items-center justify-between text-xs">
                        <span className="flex items-center gap-1">
                          <Hash className="h-3 w-3" />
                          {batch.batchNumber}
                        </span>
                        <span className={batch.balance > 0 ? 'text-green-500' : 'text-red-500'}>
                          {batch.balance} in stock
                        </span>
                      </div>
                    ))}
                    {cosmetic.batches.length > 2 && (
                      <p className="text-xs text-gray-500">+{cosmetic.batches.length - 2} more batches</p>
                    )}
                  </div>
                )}
              </div>
              <button
                onClick={() => handleAddBatch(cosmetic)}
                className="mt-3 w-full py-2 text-sm font-medium text-pink-600 hover:text-pink-700 border border-pink-200 hover:border-pink-300 rounded-lg transition-colors flex items-center justify-center gap-1"
              >
                <Plus className="h-4 w-4" /> Add Batch
              </button>
            </div>
          ))}
        </div>
      )}

      {filteredCosmetics.length === 0 && !loading && (
        <div className={`text-center py-12 rounded-xl border ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
          <Package className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
          <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No cosmetics found</p>
          <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>Add your first cosmetic product to get started</p>
        </div>
      )}

      {/* Add/Edit Cosmetic Modal */}
      <Modal isOpen={showAddModal || showEditModal} onClose={() => { setShowAddModal(false); setShowEditModal(false); }} title={showEditModal ? 'Edit Cosmetic' : 'Add New Cosmetic'}>
        <div className="space-y-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Product Name *</label>
            <input type="text" value={formData.productName} onChange={e => setFormData({ ...formData, productName: e.target.value })} className={inputClass} placeholder="e.g., Face Cream" />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Description</label>
            <textarea value={formData.description} onChange={e => setFormData({ ...formData, description: e.target.value })} className={inputClass} rows={2} placeholder="Product description" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Category</label>
              <select value={formData.cosmeticCategoryId} onChange={e => setFormData({ ...formData, cosmeticCategoryId: e.target.value })} className={inputClass}>
                <option value="">Select category</option>
                {cosmeticCategories.map(c => (
                  <option key={c.id} value={c.id}>{c.name}</option>
                ))}
              </select>
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Unit Type</label>
              <select value={formData.unitTypeId} onChange={e => setFormData({ ...formData, unitTypeId: Number(e.target.value) })} className={inputClass}>
                <option value="">Select unit</option>
                <option value="1">Piece</option>
                <option value="2">Box</option>
                <option value="3">Bottle</option>
                <option value="4">Tube</option>
              </select>
            </div>
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Price (ETB)</label>
            <input type="number" value={formData.price} onChange={e => setFormData({ ...formData, price: Number(e.target.value) })} className={inputClass} placeholder="0.00" step="0.01" min="0" />
          </div>
          {showEditModal && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isActive" checked={formData.isActive} onChange={e => setFormData({ ...formData, isActive: e.target.checked })} className="rounded" />
              <label htmlFor="isActive" className={`text-sm ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Active</label>
            </div>
          )}
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
            <button onClick={() => { setShowAddModal(false); setShowEditModal(false); }} className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>Cancel</button>
            <button onClick={showEditModal ? handleUpdate : handleSave} className="px-5 py-2.5 bg-gradient-to-r from-pink-600 to-rose-600 hover:from-pink-700 hover:to-rose-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg">
              {showEditModal ? 'Update' : 'Add'} Cosmetic
            </button>
          </div>
        </div>
      </Modal>

      {/* Add Batch Modal */}
      <Modal isOpen={showBatchModal} onClose={() => setShowBatchModal(false)} title={`Add Batch - ${selectedCosmetic?.productName}`}>
        <div className="space-y-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Batch Number *</label>
            <input type="text" value={batchData.batchNumber} onChange={e => setBatchData({ ...batchData, batchNumber: e.target.value })} className={inputClass} placeholder="e.g., BTH-001" />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Quantity</label>
            <input type="number" value={batchData.quantity} onChange={e => setBatchData({ ...batchData, quantity: Number(e.target.value) })} className={inputClass} placeholder="0" min="0" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Purchase Price</label>
              <input type="number" value={batchData.purchasePrice} onChange={e => setBatchData({ ...batchData, purchasePrice: Number(e.target.value) })} className={inputClass} placeholder="0.00" step="0.01" min="0" />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Selling Price</label>
              <input type="number" value={batchData.sellingPrice} onChange={e => setBatchData({ ...batchData, sellingPrice: Number(e.target.value) })} className={inputClass} placeholder="0.00" step="0.01" min="0" />
            </div>
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Expiry Date</label>
            <input type="date" value={batchData.expiryDate} onChange={e => setBatchData({ ...batchData, expiryDate: e.target.value })} className={inputClass} />
          </div>
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
            <button onClick={() => setShowBatchModal(false)} className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>Cancel</button>
            <button onClick={handleSaveBatch} className="px-5 py-2.5 bg-gradient-to-r from-pink-600 to-rose-600 hover:from-pink-700 hover:to-rose-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg">Add Batch</button>
          </div>
        </div>
      </Modal>
    </div>
  );
};
