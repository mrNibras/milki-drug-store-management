import React, { useState, useEffect } from 'react';
import { Plus, Edit2, Trash2, MapPin, Phone, Mail, Building2 } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Modal } from '../components/ui/Modal';
import { Badge } from '../components/ui/Badge';
import { Branch } from '../types';

export const BranchesPage: React.FC = () => {
  const { branches, fetchBranches, addBranch, updateBranch, deleteBranch, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [selectedBranch, setSelectedBranch] = useState<Branch | null>(null);
  const [formData, setFormData] = useState({
    name: '', location: '', phone: '', email: '', address: '', isActive: true,
  });

  useEffect(() => {
    fetchBranches();
  }, [fetchBranches]);

  const handleAdd = () => {
    setFormData({ name: '', location: '', phone: '', email: '', address: '', isActive: true });
    setShowAddModal(true);
  };

  const handleEdit = (branch: Branch) => {
    setSelectedBranch(branch);
    setFormData({
      name: branch.name,
      location: branch.location || '',
      phone: branch.phone || '',
      email: branch.email || '',
      address: branch.address || '',
      isActive: branch.isActive,
    });
    setShowEditModal(true);
  };

  const handleSave = async () => {
    if (!formData.name.trim()) return;
    await addBranch({
      name: formData.name,
      location: formData.location,
      phone: formData.phone,
      email: formData.email,
      address: formData.address,
      isActive: formData.isActive,
    });
    setShowAddModal(false);
  };

  const handleUpdate = async () => {
    if (!selectedBranch || !formData.name.trim()) return;
    await updateBranch(selectedBranch.id, {
      name: formData.name,
      location: formData.location,
      phone: formData.phone,
      email: formData.email,
      address: formData.address,
      isActive: formData.isActive,
    });
    setShowEditModal(false);
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Are you sure you want to deactivate this branch?')) {
      deleteBranch(id);
    }
  };

  const inputClass = `w-full px-4 py-3 border rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all ${
    isDark ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500' : 'bg-white border-gray-300 text-gray-900'
  }`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>Branch Management</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Manage pharmacy branches</p>
        </div>
        <button
          onClick={handleAdd}
          className="px-4 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg flex items-center gap-2"
        >
          <Plus className="h-4 w-4" /> Add Branch
        </button>
      </div>

      {loading ? (
        <div className="text-center py-12">
          <div className="h-8 w-8 border-2 border-blue-500 border-t-transparent rounded-full animate-spin mx-auto mb-3" />
          <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>Loading branches...</p>
        </div>
      ) : (
        <div className={`grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4`}>
          {branches.map((branch) => (
            <div key={branch.id} className={`rounded-xl border shadow-sm p-5 ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
              <div className="flex items-start justify-between mb-3">
                <div className="flex items-center gap-3">
                  <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
                    <Building2 className="h-5 w-5" />
                  </div>
                  <div>
                    <h3 className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{branch.name}</h3>
                    <Badge variant={branch.isActive ? 'success' : 'danger'}>{branch.isActive ? 'Active' : 'Inactive'}</Badge>
                  </div>
                </div>
                <div className="flex gap-1">
                  <button onClick={() => handleEdit(branch)} className={`p-2 rounded-lg transition-colors ${isDark ? 'hover:bg-amber-900/30 text-gray-400 hover:text-amber-400' : 'hover:bg-amber-50 text-gray-400 hover:text-amber-600'}`}>
                    <Edit2 className="h-4 w-4" />
                  </button>
                  <button onClick={() => handleDelete(branch.id)} className={`p-2 rounded-lg transition-colors ${isDark ? 'hover:bg-red-900/30 text-gray-400 hover:text-red-400' : 'hover:bg-red-50 text-gray-400 hover:text-red-600'}`}>
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
              <div className={`space-y-2 text-sm ${isDark ? 'text-gray-400' : 'text-gray-600'}`}>
                {branch.location && (
                  <div className="flex items-center gap-2">
                    <MapPin className="h-4 w-4" />
                    {branch.location}
                  </div>
                )}
                {branch.phone && (
                  <div className="flex items-center gap-2">
                    <Phone className="h-4 w-4" />
                    {branch.phone}
                  </div>
                )}
                {branch.email && (
                  <div className="flex items-center gap-2">
                    <Mail className="h-4 w-4" />
                    {branch.email}
                  </div>
                )}
                {branch.address && (
                  <div className="flex items-start gap-2">
                    <Building2 className="h-4 w-4 mt-0.5" />
                    <span className="line-clamp-2">{branch.address}</span>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {branches.length === 0 && !loading && (
        <div className={`text-center py-12 rounded-xl border ${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-200'}`}>
          <Building2 className={`h-12 w-12 mx-auto mb-3 ${isDark ? 'text-gray-600' : 'text-gray-300'}`} />
          <p className={isDark ? 'text-gray-400' : 'text-gray-500'}>No branches found</p>
          <p className={`text-sm mt-1 ${isDark ? 'text-gray-500' : 'text-gray-400'}`}>Create your first branch to get started</p>
        </div>
      )}

      {/* Add Branch Modal */}
      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Add New Branch">
        <div className="space-y-4">
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Branch Name *</label>
            <input type="text" value={formData.name} onChange={e => setFormData({ ...formData, name: e.target.value })} className={inputClass} placeholder="e.g., Main Branch" />
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Location</label>
            <input type="text" value={formData.location} onChange={e => setFormData({ ...formData, location: e.target.value })} className={inputClass} placeholder="e.g., Addis Ababa" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Phone</label>
              <input type="text" value={formData.phone} onChange={e => setFormData({ ...formData, phone: e.target.value })} className={inputClass} placeholder="+251911223344" />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Email</label>
              <input type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} className={inputClass} placeholder="branch@milki.com" />
            </div>
          </div>
          <div>
            <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Address</label>
            <textarea value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} className={inputClass} rows={2} placeholder="Full address" />
          </div>
          <div className="flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}">
            <button onClick={() => setShowAddModal(false)} className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>Cancel</button>
            <button onClick={handleSave} className="px-5 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg">Add Branch</button>
          </div>
        </div>
      </Modal>

      {/* Edit Branch Modal */}
      <Modal isOpen={showEditModal} onClose={() => setShowEditModal(false)} title="Edit Branch">
        {selectedBranch && (
          <div className="space-y-4">
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Branch Name *</label>
              <input type="text" value={formData.name} onChange={e => setFormData({ ...formData, name: e.target.value })} className={inputClass} />
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Location</label>
              <input type="text" value={formData.location} onChange={e => setFormData({ ...formData, location: e.target.value })} className={inputClass} />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Phone</label>
                <input type="text" value={formData.phone} onChange={e => setFormData({ ...formData, phone: e.target.value })} className={inputClass} />
              </div>
              <div>
                <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Email</label>
                <input type="email" value={formData.email} onChange={e => setFormData({ ...formData, email: e.target.value })} className={inputClass} />
              </div>
            </div>
            <div>
              <label className={`block text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Address</label>
              <textarea value={formData.address} onChange={e => setFormData({ ...formData, address: e.target.value })} className={inputClass} rows={2} />
            </div>
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isActive" checked={formData.isActive} onChange={e => setFormData({ ...formData, isActive: e.target.checked })} className="rounded" />
              <label htmlFor="isActive" className={`text-sm ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>Active</label>
            </div>
            <div className="flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}">
              <button onClick={() => setShowEditModal(false)} className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'}`}>Cancel</button>
              <button onClick={handleUpdate} className="px-5 py-2.5 bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg">Update Branch</button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};
