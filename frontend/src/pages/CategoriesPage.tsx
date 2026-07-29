import React, { useState, useEffect } from 'react';
import { Plus, Edit2, Trash2, Package, Search, X } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Modal } from '../components/ui/Modal';
import { formatDate, generateId } from '../utils/helpers';
import { Category } from '../types';

export const CategoriesPage: React.FC = () => {
  const { categories, unitTypes, fetchCategories, fetchUnitTypes, currentUser } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';
  const [search, setSearch] = useState('');
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedCategory, setSelectedCategory] = useState<Category | null>(null);
  const [formData, setFormData] = useState({ name: '', unitTypeId: 1, isActive: true });

  useEffect(() => {
    fetchCategories();
    fetchUnitTypes();
  }, [fetchCategories, fetchUnitTypes]);

  const filteredCategories = (categories || []).filter(c =>
    c.name.toLowerCase().includes(search.toLowerCase())
  );

  const handleAdd = () => {
    setFormData({ name: '', unitTypeId: 1, isActive: true });
    setShowAddModal(true);
  };

  const handleEdit = (category: Category) => {
    setSelectedCategory(category);
    setFormData({ name: category.name, unitTypeId: category.unitTypeId || 1, isActive: category.isActive });
    setShowEditModal(true);
  };

  const handleSave = async () => {
    if (!formData.name.trim()) return;

    if (selectedCategory) {
      await useAppStore.getState().updateCategory(selectedCategory.id, {
        name: formData.name,
        unitTypeId: formData.unitTypeId,
        isActive: formData.isActive,
      });
      setShowEditModal(false);
    } else {
      await useAppStore.getState().addCategory({
        id: generateId(),
        name: formData.name,
        unitTypeId: formData.unitTypeId,
        isActive: formData.isActive,
        createdAt: new Date().toISOString(),
      });
      setShowAddModal(false);
    }
    await fetchCategories();
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this category?')) return;
    await useAppStore.getState().deleteCategory(id);
    await fetchCategories();
  };

  const getUnitTypeName = (unitTypeId: number) => {
    return (unitTypes || []).find(u => u.id === String(unitTypeId))?.name || 'Unknown';
  };

  const inputClass = `w-full px-3 py-2 border rounded-lg focus:ring-2 focus:ring-blue-500 outline-none text-sm ${
    isDark ? 'bg-gray-700 border-gray-600 text-white placeholder-gray-400' : 'bg-white border-gray-300 text-gray-900'
  }`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Categories</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Manage medicine categories and unit types</p>
        </div>
        <button
          onClick={handleAdd}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors"
        >
          <Plus className="h-4 w-4" /> Add Category
        </button>
      </div>

      <div className={`p-4 rounded-xl border ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'}`}>
        <div className="relative">
          <Search className={`absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 ${isDark ? 'text-gray-500' : 'text-gray-400'}`} />
          <input
            type="text"
            placeholder="Search categories..."
            value={search}
            onChange={e => setSearch(e.target.value)}
            className={`${inputClass} pl-9`}
          />
        </div>
      </div>

      <div className={`rounded-xl border overflow-hidden ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'}`}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Name</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Unit Type</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Created</th>
                <th className={`text-right px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Actions</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {(filteredCategories || []).length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-12 text-center">
                    <Package className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
                    <p className={`font-medium ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>No categories found</p>
                  </td>
                </tr>
              ) : (
                (filteredCategories || []).map(category => (
                  <tr key={category.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                    <td className={`px-6 py-3 text-sm font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{category.name}</td>
                    <td className="px-6 py-3">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${
                        isDark ? 'bg-gray-700 text-gray-300' : 'bg-gray-100 text-gray-700'
                      }`}>
                        {category.unitTypeName || getUnitTypeName(category.unitTypeId)}
                      </span>
                    </td>
                    <td className="px-6 py-3">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${
                        category.isActive
                          ? isDark ? 'bg-emerald-900/30 text-emerald-400' : 'bg-emerald-100 text-emerald-700'
                          : isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-700'
                      }`}>
                        {category.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className={`px-6 py-3 text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>
                      {formatDate(category.createdAt)}
                    </td>
                    <td className="px-6 py-3 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <button
                          onClick={() => handleEdit(category)}
                          className={`p-2 rounded-lg transition-colors ${
                            isDark ? 'hover:bg-gray-700 text-gray-400 hover:text-blue-400' : 'hover:bg-gray-100 text-gray-400 hover:text-blue-600'
                          }`}
                        >
                          <Edit2 className="h-4 w-4" />
                        </button>
                        <button
                          onClick={() => handleDelete(category.id)}
                          className={`p-2 rounded-lg transition-colors ${
                            isDark ? 'hover:bg-gray-700 text-gray-400 hover:text-red-400' : 'hover:bg-gray-100 text-gray-400 hover:text-red-600'
                          }`}
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Add Category">
        <div className="space-y-4">
          <div>
            <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Category Name</label>
            <input
              type="text"
              value={formData.name}
              onChange={e => setFormData({ ...formData, name: e.target.value })}
              className={inputClass}
              placeholder="e.g. Antibiotic"
            />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Unit Type</label>
            <select
              value={formData.unitTypeId}
              onChange={e => setFormData({ ...formData, unitTypeId: Number(e.target.value) })}
              className={inputClass}
            >
              {(unitTypes || []).map(u => (
                <option key={u.id} value={Number(u.id)}>{u.name}</option>
              ))}
            </select>
          </div>
          <div className="flex justify-end gap-3 pt-4">
            <button onClick={() => setShowAddModal(false)} className={`px-4 py-2 rounded-lg ${isDark ? 'border border-gray-600 text-gray-300' : 'border border-gray-300 text-gray-700'}`}>Cancel</button>
            <button onClick={handleSave} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg">Save</button>
          </div>
        </div>
      </Modal>

      <Modal isOpen={showEditModal} onClose={() => setShowEditModal(false)} title="Edit Category">
        <div className="space-y-4">
          <div>
            <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Category Name</label>
            <input
              type="text"
              value={formData.name}
              onChange={e => setFormData({ ...formData, name: e.target.value })}
              className={inputClass}
            />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-1 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Unit Type</label>
            <select
              value={formData.unitTypeId}
              onChange={e => setFormData({ ...formData, unitTypeId: Number(e.target.value) })}
              className={inputClass}
            >
              {(unitTypes || []).map(u => (
                <option key={u.id} value={Number(u.id)}>{u.name}</option>
              ))}
            </select>
          </div>
          <div className="flex items-center gap-2">
            <input
              type="checkbox"
              id="isActive"
              checked={formData.isActive}
              onChange={e => setFormData({ ...formData, isActive: e.target.checked })}
              className="rounded"
            />
            <label htmlFor="isActive" className={`text-sm ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Active</label>
          </div>
          <div className="flex justify-end gap-3 pt-4">
            <button onClick={() => setShowEditModal(false)} className={`px-4 py-2 rounded-lg ${isDark ? 'border border-gray-600 text-gray-300' : 'border border-gray-300 text-gray-700'}`}>Cancel</button>
            <button onClick={handleSave} className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg">Update</button>
          </div>
        </div>
      </Modal>
    </div>
  );
};
