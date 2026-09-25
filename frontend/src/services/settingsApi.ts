import { api, SettingsResponse } from './api';

export interface PublicSettingsResponse {
  pharmacyName: string;
  language: string;
  currency: string;
}

export const getSettings = async (): Promise<SettingsResponse> => {
  const res = await api.get<SettingsResponse>('/settings');
  return res.data;
};

export const getPublicSettings = async (): Promise<PublicSettingsResponse> => {
  const res = await api.get<PublicSettingsResponse>('/settings/public');
  return res.data;
};

export const updateSettings = async (data: Partial<SettingsResponse>): Promise<SettingsResponse> => {
  const res = await api.put<SettingsResponse>('/settings', data);
  return res.data;
};
