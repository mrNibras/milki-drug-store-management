import React, { useState, useEffect } from 'react';
import { Users, Shield, UserCheck, UserX, Plus, Edit2, Trash2, Mail } from 'lucide-react';
import { useAppStore } from '../store/appStore';
import { useThemeStore } from '../store/themeStore';
import { Button } from '../components/ui/Button';
import { Badge } from '../components/ui/Badge';
import { Modal } from '../components/ui/Modal';
import { formatDate, generateId } from '../utils/helpers';
import { User } from '../types';

export const UsersPage: React.FC = () => {
  const { users, fetchUsers, addUser, updateUser, deleteUser, toggleUserActive, currentUser, loading } = useAppStore();
  const { theme } = useThemeStore();
  const isDark = theme === 'dark';

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);
  const [showAddModal, setShowAddModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | null>(null);
  const [formData, setFormData] = useState({ 
    fullName: '', 
    email: '', 
    role: 'pharmacist' as 'admin' | 'pharmacist',
    password: '',
    confirmPassword: '',
  });
  const [passwordError, setPasswordError] = useState('');

  const activeUsers = users.filter(u => u.isActive).length;
  const inactiveUsers = users.filter(u => !u.isActive).length;

  const resetForm = () => {
    setFormData({ fullName: '', email: '', role: 'pharmacist', password: '', confirmPassword: '' });
    setPasswordError('');
  };

  const validatePassword = () => {
    if (!formData.password) {
      setPasswordError('Password is required');
      return false;
    }
    if (formData.password.length < 6) {
      setPasswordError('Password must be at least 6 characters');
      return false;
    }
    if (formData.password !== formData.confirmPassword) {
      setPasswordError('Passwords do not match');
      return false;
    }
    setPasswordError('');
    return true;
  };

  const handleAddUser = () => {
    if (!formData.fullName || !formData.email) return;
    if (!validatePassword()) return;

    addUser({
      fullName: formData.fullName,
      email: formData.email,
      role: formData.role,
      password: formData.password,
    } as any);
    setShowAddModal(false);
    resetForm();
  };

  const handleEditClick = (user: User) => {
    setSelectedUser(user);
    setFormData({
      fullName: user.fullName,
      email: user.email,
      role: user.role,
      password: '',
      confirmPassword: '',
    });
    setShowEditModal(true);
  };

  const handleUpdateUser = () => {
    if (!selectedUser) return;
    updateUser(selectedUser.id, {
      fullName: formData.fullName,
      email: formData.email,
      role: formData.role,
    });
    setShowEditModal(false);
    setSelectedUser(null);
    resetForm();
  };

  const handleDeleteClick = (user: User) => {
    setSelectedUser(user);
    setShowDeleteModal(true);
  };

  const handleConfirmDelete = () => {
    if (!selectedUser) return;
    deleteUser(selectedUser.id);
    setShowDeleteModal(false);
    setSelectedUser(null);
  };

  const handleToggleActive = (userId: string) => {
    const user = users.find(u => u.id === userId);
    if (!user || userId === currentUser?.id) return;
    toggleUserActive(userId);
  };

  const inputClass = `w-full px-4 py-3 border rounded-xl focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-all ${
    isDark 
      ? 'bg-gray-800 border-gray-600 text-white placeholder-gray-500 focus:bg-gray-700' 
      : 'bg-white border-gray-300 text-gray-900 placeholder-gray-400'
  }`;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className={`text-2xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>User Management</h1>
          <p className={`${isDark ? 'text-gray-400' : 'text-gray-500'} mt-1`}>Manage system users and roles</p>
        </div>
        <Button onClick={() => { resetForm(); setShowAddModal(true); }}>
          <Plus className="h-4 w-4" /> Add User
        </Button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'}`}>
              <Users className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Total Users</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{users.length}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-green-900/30 text-green-400' : 'bg-green-100 text-green-600'}`}>
              <UserCheck className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Active</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{activeUsers}</p>
            </div>
          </div>
        </div>
        <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border p-4`}>
          <div className="flex items-center gap-3">
            <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-600'}`}>
              <UserX className="h-5 w-5" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Inactive</p>
              <p className={`text-xl font-bold ${isDark ? 'text-white' : 'text-gray-900'}`}>{inactiveUsers}</p>
            </div>
          </div>
        </div>
      </div>

      {/* Users Table */}
      <div className={`${isDark ? 'bg-gray-800 border-gray-700' : 'bg-white border-gray-100'} rounded-xl border overflow-hidden`}>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className={isDark ? 'bg-gray-700' : 'bg-gray-50'}>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>User</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Email</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Role</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Status</th>
                <th className={`text-left px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Created</th>
                <th className={`text-right px-6 py-3 text-xs font-semibold uppercase ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Actions</th>
              </tr>
            </thead>
            <tbody className={`divide-y ${isDark ? 'divide-gray-700' : 'divide-gray-100'}`}>
              {users.map(user => (
                <tr key={user.id} className={isDark ? 'hover:bg-gray-700/50' : 'hover:bg-gray-50'}>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className={`flex h-10 w-10 items-center justify-center rounded-full text-sm font-bold text-white ${
                        user.role === 'admin' ? 'bg-gradient-to-br from-purple-500 to-indigo-600' : 'bg-gradient-to-br from-blue-400 to-cyan-500'
                      }`}>
                         {user.fullName ? user.fullName.charAt(0) : '?'}
                      </div>
                      <div>
                        <p className={`font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{user.fullName}</p>
                        {user.id === currentUser?.id && (
                          <span className="text-xs text-blue-500">(You)</span>
                        )}
                      </div>
                    </div>
                  </td>
                  <td className={`px-6 py-4 text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>{user.email}</td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-1.5">
                      {user.role === 'admin' ? (
                        <Shield className="h-4 w-4 text-purple-500" />
                      ) : (
                        <Users className="h-4 w-4 text-blue-500" />
                      )}
                      <span className={`text-sm font-medium capitalize ${isDark ? 'text-gray-200' : 'text-gray-900'}`}>{user.role}</span>
                    </div>
                  </td>
                  <td className="px-6 py-4">
                    <Badge variant={user.isActive ? 'success' : 'danger'}>
                      {user.isActive ? 'Active' : 'Inactive'}
                    </Badge>
                  </td>
                  <td className={`px-6 py-4 text-sm ${isDark ? 'text-gray-300' : 'text-gray-600'}`}>{formatDate(user.createdAt)}</td>
                  <td className="px-6 py-4">
                    <div className="flex items-center justify-end gap-1">
                      {/* Edit Button */}
                      <button
                        onClick={() => handleEditClick(user)}
                        className={`p-2 rounded-lg transition-colors ${
                          isDark ? 'hover:bg-blue-900/30 text-gray-400 hover:text-blue-400' : 'hover:bg-blue-50 text-gray-400 hover:text-blue-600'
                        }`}
                        title="Edit User"
                      >
                        <Edit2 className="h-4 w-4" />
                      </button>
                      
                      {/* Activate/Deactivate Button */}
                      {user.id !== currentUser?.id && (
                        <button
                          onClick={() => handleToggleActive(user.id)}
                          className={`p-2 rounded-lg transition-colors ${
                            user.isActive
                              ? isDark ? 'hover:bg-amber-900/30 text-gray-400 hover:text-amber-400' : 'hover:bg-amber-50 text-gray-400 hover:text-amber-600'
                              : isDark ? 'hover:bg-green-900/30 text-gray-400 hover:text-green-400' : 'hover:bg-green-50 text-gray-400 hover:text-green-600'
                          }`}
                          title={user.isActive ? 'Deactivate' : 'Activate'}
                        >
                          {user.isActive ? <UserX className="h-4 w-4" /> : <UserCheck className="h-4 w-4" />}
                        </button>
                      )}
                      
                      {/* Delete Button */}
                      {user.id !== currentUser?.id && (
                        <button
                          onClick={() => handleDeleteClick(user)}
                          className={`p-2 rounded-lg transition-colors ${
                            isDark ? 'hover:bg-red-900/30 text-gray-400 hover:text-red-400' : 'hover:bg-red-50 text-gray-400 hover:text-red-600'
                          }`}
                          title="Delete User"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Add User Modal */}
      <Modal isOpen={showAddModal} onClose={() => setShowAddModal(false)} title="Add New User">
        <div className="space-y-5">
          {/* User Icon */}
          <div className="flex items-center gap-4 pb-2">
            <div className={`flex h-14 w-14 items-center justify-center rounded-xl ${
              isDark ? 'bg-blue-900/30 text-blue-400' : 'bg-blue-100 text-blue-600'
            }`}>
              <Users className="h-7 w-7" />
            </div>
            <div>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Create a new user account</p>
            </div>
          </div>

          {/* Form */}
          <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
            <h4 className={`text-sm font-semibold mb-4 ${isDark ? 'text-gray-200' : 'text-gray-700'}`}>User Information</h4>
            <div className="space-y-4">
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  <Users className="h-4 w-4" />
                  Full Name <span className="text-red-500">*</span>
                </label>
                <input
                  type="text"
                  value={formData.fullName}
                  onChange={e => setFormData({ ...formData, fullName: e.target.value })}
                  className={inputClass}
                  placeholder="Enter full name"
                />
               </div>
               <div>
                 <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                   <Mail className="h-4 w-4" />
                   Email Address <span className="text-red-500">*</span>
                 </label>
                 <input
                   type="email"
                   value={formData.email}
                   onChange={e => setFormData({ ...formData, email: e.target.value })}
                   className={inputClass}
                   placeholder="user@email.com"
                 />
               </div>
               <div>
                 <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                   Password <span className="text-red-500">*</span>
                 </label>
                 <input
                   type="password"
                   value={formData.password}
                   onChange={e => setFormData({ ...formData, password: e.target.value })}
                   className={inputClass}
                   placeholder="Minimum 6 characters"
                 />
               </div>
               <div>
                 <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                   Confirm Password <span className="text-red-500">*</span>
                 </label>
                 <input
                   type="password"
                   value={formData.confirmPassword}
                   onChange={e => setFormData({ ...formData, confirmPassword: e.target.value })}
                   className={inputClass}
                   placeholder="Re-enter password"
                 />
                 {passwordError && (
                   <p className="text-xs text-red-500 mt-1">{passwordError}</p>
                 )}
               </div>
             </div>
           </div>

          <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
            <h4 className={`text-sm font-semibold mb-4 ${isDark ? 'text-gray-200' : 'text-gray-700'}`}>Role & Permissions</h4>
            <div>
              <label className={`flex items-center gap-2 text-sm font-medium mb-3 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                <Shield className="h-4 w-4" />
                Select Role
              </label>
              <div className="grid grid-cols-2 gap-3">
                <button
                  type="button"
                  onClick={() => setFormData({ ...formData, role: 'admin' })}
                  className={`p-4 rounded-xl text-left transition-all ${
                    formData.role === 'admin'
                      ? 'bg-purple-600 text-white shadow-lg shadow-purple-200 dark:shadow-purple-900/30'
                      : isDark
                      ? 'bg-gray-800 text-gray-300 border border-gray-600 hover:border-purple-500'
                      : 'bg-white text-gray-700 border border-gray-300 hover:border-purple-400'
                  }`}
                >
                  <div className="flex items-center gap-2 mb-2">
                    <Shield className="h-5 w-5" />
                    <span className="font-semibold">Admin</span>
                  </div>
                  <p className="text-xs opacity-80">Full system access, manage users and settings</p>
                </button>
                <button
                  type="button"
                  onClick={() => setFormData({ ...formData, role: 'pharmacist' })}
                  className={`p-4 rounded-xl text-left transition-all ${
                    formData.role === 'pharmacist'
                      ? 'bg-blue-600 text-white shadow-lg shadow-blue-200 dark:shadow-blue-900/30'
                      : isDark
                      ? 'bg-gray-800 text-gray-300 border border-gray-600 hover:border-blue-500'
                      : 'bg-white text-gray-700 border border-gray-300 hover:border-blue-400'
                  }`}
                >
                  <div className="flex items-center gap-2 mb-2">
                    <Users className="h-5 w-5" />
                    <span className="font-semibold">Pharmacist</span>
                  </div>
                  <p className="text-xs opacity-80">Sales, inventory view, damage/expiry recording</p>
                </button>
              </div>
            </div>
          </div>

          <div className={`rounded-lg p-4 ${isDark ? 'bg-blue-900/20 border border-blue-800' : 'bg-blue-50 border border-blue-200'}`}>
            <p className={`text-sm ${isDark ? 'text-blue-300' : 'text-blue-700'}`}>
              The user will receive an email with their login credentials.
            </p>
          </div>

          {/* Actions */}
          <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
            <button
              onClick={() => setShowAddModal(false)}
              className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${
                isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
              }`}
            >
              Cancel
            </button>
            <button
              onClick={handleAddUser}
              className="px-5 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg shadow-blue-200 dark:shadow-blue-900/30 flex items-center gap-2"
            >
              <Plus className="h-4 w-4" />
              Add User
            </button>
          </div>
        </div>
      </Modal>

      {/* Edit User Modal */}
      <Modal isOpen={showEditModal} onClose={() => setShowEditModal(false)} title="Edit User">
        {selectedUser && (
          <div className="space-y-5">
            {/* User Info Header */}
            <div className="flex items-center gap-4 pb-2">
              <div className={`flex h-14 w-14 items-center justify-center rounded-xl text-2xl font-bold text-white ${
                selectedUser.role === 'admin' ? 'bg-gradient-to-br from-purple-500 to-indigo-600' : 'bg-gradient-to-br from-blue-400 to-cyan-500'
              }`}>
                   {selectedUser.fullName ? selectedUser.fullName.charAt(0) : '?'}
              </div>
              <div>
                <p className={`font-semibold ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedUser.fullName}</p>
                <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>Editing user profile</p>
              </div>
            </div>

            {/* Form */}
            <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
              <h4 className={`text-sm font-semibold mb-4 ${isDark ? 'text-gray-200' : 'text-gray-700'}`}>User Information</h4>
              <div className="space-y-4">
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    <Users className="h-4 w-4" />
                    Full Name
                  </label>
                  <input
                    type="text"
                    value={formData.fullName}
                    onChange={e => setFormData({ ...formData, fullName: e.target.value })}
                    className={inputClass}
                  />
                </div>
                <div>
                  <label className={`flex items-center gap-2 text-sm font-medium mb-2 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                    <Mail className="h-4 w-4" />
                    Email Address
                  </label>
                  <input
                    type="email"
                    value={formData.email}
                    onChange={e => setFormData({ ...formData, email: e.target.value })}
                    className={inputClass}
                  />
                </div>
              </div>
            </div>

            <div className={`p-5 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
              <h4 className={`text-sm font-semibold mb-4 ${isDark ? 'text-gray-200' : 'text-gray-700'}`}>Role & Permissions</h4>
              <div>
                <label className={`flex items-center gap-2 text-sm font-medium mb-3 ${isDark ? 'text-gray-300' : 'text-gray-700'}`}>
                  <Shield className="h-4 w-4" />
                  Select Role
                </label>
                <div className="grid grid-cols-2 gap-3">
                  <button
                    type="button"
                    onClick={() => setFormData({ ...formData, role: 'admin' })}
                    className={`p-4 rounded-xl text-left transition-all ${
                      formData.role === 'admin'
                        ? 'bg-purple-600 text-white shadow-lg shadow-purple-200 dark:shadow-purple-900/30'
                        : isDark
                        ? 'bg-gray-800 text-gray-300 border border-gray-600 hover:border-purple-500'
                        : 'bg-white text-gray-700 border border-gray-300 hover:border-purple-400'
                    }`}
                  >
                    <div className="flex items-center gap-2 mb-2">
                      <Shield className="h-5 w-5" />
                      <span className="font-semibold">Admin</span>
                    </div>
                    <p className="text-xs opacity-80">Full system access</p>
                  </button>
                  <button
                    type="button"
                    onClick={() => setFormData({ ...formData, role: 'pharmacist' })}
                    className={`p-4 rounded-xl text-left transition-all ${
                      formData.role === 'pharmacist'
                        ? 'bg-blue-600 text-white shadow-lg shadow-blue-200 dark:shadow-blue-900/30'
                        : isDark
                        ? 'bg-gray-800 text-gray-300 border border-gray-600 hover:border-blue-500'
                        : 'bg-white text-gray-700 border border-gray-300 hover:border-blue-400'
                    }`}
                  >
                    <div className="flex items-center gap-2 mb-2">
                      <Users className="h-5 w-5" />
                      <span className="font-semibold">Pharmacist</span>
                    </div>
                    <p className="text-xs opacity-80">Limited access</p>
                  </button>
                </div>
              </div>
            </div>

            {/* Actions */}
            <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
              <button
                onClick={() => setShowEditModal(false)}
                className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${
                  isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                Cancel
              </button>
              <button
                onClick={handleUpdateUser}
                className="px-5 py-2.5 bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg shadow-blue-200 dark:shadow-blue-900/30 flex items-center gap-2"
              >
                <Edit2 className="h-4 w-4" />
                Update User
              </button>
            </div>
          </div>
        )}
      </Modal>

      {/* Delete Confirmation Modal */}
      <Modal isOpen={showDeleteModal} onClose={() => setShowDeleteModal(false)} title="Delete User" size="sm">
        {selectedUser && (
          <div className="space-y-5">
            <div className="text-center">
              <div className={`mx-auto flex h-16 w-16 items-center justify-center rounded-full mb-4 ${
                isDark ? 'bg-red-900/30 text-red-400' : 'bg-red-100 text-red-600'
              }`}>
                <Trash2 className="h-8 w-8" />
              </div>
              <h3 className={`text-lg font-semibold mb-2 ${isDark ? 'text-white' : 'text-gray-900'}`}>
                Delete {selectedUser.fullName}?
              </h3>
              <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>
                This action cannot be undone. All data associated with this user will be permanently removed.
              </p>
            </div>

            <div className={`p-4 rounded-xl ${isDark ? 'bg-gray-700/50' : 'bg-gray-50'}`}>
              <div className="flex items-center gap-3">
                <div className={`flex h-10 w-10 items-center justify-center rounded-full text-sm font-bold text-white ${
                  selectedUser.role === 'admin' ? 'bg-gradient-to-br from-purple-500 to-indigo-600' : 'bg-gradient-to-br from-blue-400 to-cyan-500'
                }`}>
                  {selectedUser.fullName ? selectedUser.fullName.charAt(0) : '?'}
                </div>
                <div>
                  <p className={`font-medium ${isDark ? 'text-white' : 'text-gray-900'}`}>{selectedUser.fullName}</p>
                  <p className={`text-sm ${isDark ? 'text-gray-400' : 'text-gray-500'}`}>{selectedUser.email}</p>
                </div>
              </div>
            </div>

            <div className={`flex justify-end gap-3 pt-4 border-t ${isDark ? 'border-gray-700' : 'border-gray-200'}`}>
              <button
                onClick={() => setShowDeleteModal(false)}
                className={`px-5 py-2.5 rounded-xl text-sm font-medium transition-colors ${
                  isDark ? 'bg-gray-700 text-gray-300 hover:bg-gray-600' : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                }`}
              >
                Cancel
              </button>
              <button
                onClick={handleConfirmDelete}
                className="px-5 py-2.5 bg-gradient-to-r from-red-600 to-rose-600 hover:from-red-700 hover:to-rose-700 text-white rounded-xl text-sm font-medium transition-all shadow-lg shadow-red-200 dark:shadow-red-900/30 flex items-center gap-2"
              >
                <Trash2 className="h-4 w-4" />
                Delete User
              </button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
};
