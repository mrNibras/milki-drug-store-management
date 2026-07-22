import React, { useState, useEffect } from 'react';
import { Plus, Edit2, Trash2, Search, Tag, AlertCircle } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Modal } from '../components/ui/Modal';
import { Badge } from '../components/ui/Badge';
import { CosmeticCategory } from '../types';

export const CosmeticCategoriesPage: React.FC = () => {
  const { cosmeticCategories, fetchCosmeticCategories, addCosmeticCategory, updateCosmeticCategory, deleteCosmeticCategory, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<CosmeticCategory | null>(null);
  const [search, setSearch] = useState('');
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    isActive: true,
  });
  const [error, setError] = useState('');

  useEffect(() => {
    fetchCosmeticCategories();
  }, [fetchCosmeticCategories]);

  const filteredCategories = cosmeticCategories.filter(c =>
    c.name.toLowerCase().includes(search.toLowerCase()) ||
    (c.description || '').toLowerCase().includes(search.toLowerCase())
  );

  const handleAdd = () => {
    setFormData({ name: '', description: '', isActive: true });
    setShowAddModal(true);
    setError('');
  };

  const handleEdit = (category: CosmeticCategory) => {
    setSelectedCategory(category);
    setFormData({
      name: category.name,
      description: category.description || '',
      isActive: category.isActive,
    });
    setShowEditModal(true);
    setError('');
  };

  const handleSave = async () => {
    if (!formData.name.trim()) return;
    try {
      await addCosmeticCategory({
        id: '',
        name: formData.name,
        description: formData.description,
        isActive: formData.isActive,
        createdAt: new Date().toISOString(),
      });
      setShowAddModal(false);
      setFormData({ name: '', description: '', isActive: true });
    } catch (e: any) {
      setError(e.response?.data?.message || 'Failed to add category');
    }
  };

  const handleUpdate = async () => {
    if (!selectedCategory || !formData.name.trim()) return;
    try {
      await updateCosmeticCategory(selectedCategory.id, formData);
      setShowEditModal(false);
      setSelectedCategory(null);
      setFormData({ name: '', description: '', isActive: true });
    } catch (e: any) {
      setError(e.response?.data?.message || 'Failed to update category');
    }
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to deactivate this category?')) {
      deleteCosmeticCategory(id);
    }
  };

  const inputClass = `w-full px-4 py-3 border rounded-xl focus:ring-2 focus:ring-pink-500 focus:border-pink-500 transition-all ${
    isDark ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500' : 'bg-white border-gray-300 text-gray-900'
  }`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Cosmetic Categories</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Manage cosmetic product categories</p>
        </div>
        <button
          onClick={handleAdd}
          className="px-4 py-2.5 bg-gradient-to-r from-pink-600 to-rose-600 hover:from-pink-700 hover:to-rose-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2"
        >
          <Plus className="h-4 w-4" /> Add Category
        </button>
      </div>

      <div className="relative">
        <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search categories..."
          className={`w-full pl-10 pr-4 py-3 border rounded-xl focus:ring-2 focus:ring-pink-500 focus:border-pink-500 transition-all ${isDark ? 'bg-gray-800 border-gray-700 text-white placeholder-gray-500' : 'border-gray-300 text-gray-900'}`}
        />
      </div>

      {error && (
        <div className={`flex items-center gap-2 rounded-lg p-3 ${isDark ? 'bg-red-900/30 border border-red-800' : 'bg-red-50 border border-red-200'}`}>
          <AlertCircle className="h-5 w-5 text-red-500 flex-shrink-0" />
          <p className="text-sm text-red-500">{error}</p>
        </div>
      )}

      {loading ? (
        <div className="text-center py-12">
          <div className="h-8 w-8 border-2 border-pink-500 border-t-transparent rounded-full animate-spin mx-auto mb-3" />
          <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>Loading categories...</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {filteredCategories.map((category) => (
            <div key={category.id} className={`rounded-xl border shadow-sm p-5 ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-3">
                  <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-pink-900/30 text-pink-400' : 'bg-pink-100 text-pink-600'}`}>
                    <Tag className="h-5 w-5" />
                  </div>
                  <div>
                    <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{category.name}</h3>
                    <Badge variant={category.isActive ? 'success' : 'danger'}>{category.isActive ? 'Active' : 'Inactive'}</Badge>
                  </div>
                </div>
                <div className="flex gap-1">
                  <button onClick={() => handleEdit(category)} className={`p-2 rounded-lg transition-colors ${isDark ? 'hover:bg-amber-900/30 text-gray-400 hover:text-amber-400' : 'hover:bg-amber-50 text-gray-400 hover:text-amber-600'}`}>
                    <Edit2 className="h-4 w-4" />
                  </button>
                  <button onClick={() => handleDelete(category.id)} className={`p-2 rounded-lg transition-colors ${isDark ? 'hover:bg-red-900/30 text-gray-400 hover:text-red-400' : 'hover:bg-red-50 text-gray-400 hover:text-red-600'}`}>
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
              {category.description && (
                <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>{category.description}</p>
              )}
            </div>
          ))}
        </div>
      )}

      {filteredCategories.length === 0 && !loading && (
        <div className={`text-center py-12 rounded-xl border ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
          <Tag className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
          <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No categories found</p>
          <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>Add your first cosmetic category to get started</p>
        </div>
      )}

      {/* Add/Edit Category Modal */}
      <Modal isOpen={showAddModal || showEditModal} onClose={() => { setShowAddModal(false); setShowEditModal(false); setError(''); }} title={showEditModal ? 'Edit Category' : 'Add New Category'}>
        <div className="space-y-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Category Name *</label>
            <input type="text" value={formData.name} onChange={e => setFormData({ ...formData, name: e.target.value })} className={inputClass} placeholder="e.g., Skin Care" />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Description</label>
            <textarea value={formData.description} onChange={e => setFormData({ ...formData, description: e.target.value })} className={inputClass} rows={2} placeholder="Optional description" />
          </div>
          {showEditModal && (
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isActive" checked={formData.isActive} onChange={e => setFormData({ ...formData, isActive: e.target.checked })} className="rounded" />
              <label htmlFor="isActive" className={`text-sm ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Active</label>
            </div>
          )}
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
            <button onClick={() => { setShowAddModal(false); setShowEditModal(false); setError(''); }} className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>Cancel</button>
            <button onClick={showEditModal ? handleUpdate : handleSave} className="px-5 py-2.5 bg-gradient-to-r from-pink-600 to-rose-600 hover:from-pink-700 hover:to-rose-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg">
              {showEditModal ? 'Update' : 'Add'} Category
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
};
