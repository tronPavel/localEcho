import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Modal } from '@/shared/ui/Modal/Modal';
import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { Select } from '@/shared/ui/Select/Select'; // Кастомный селект
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { useAuthStore } from '../model/authStore';
import { logout } from '../model/authApi';
import { getMyProfile, updateProfile } from '@/entities/user/model/userApi';
import { getDistricts } from '@/entities/district/model/districtApi';
import type { DistrictDto } from '../model/types';
import styles from './ProfileModal.module.css';
import type {UpdateProfileDto} from "@/entities/user/model/types.ts";

interface ProfileModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export const ProfileModal = ({ isOpen, onClose }: ProfileModalProps) => {
    const { user, clearUser } = useAuthStore();
    const queryClient = useQueryClient();

    // 1. Получаем полные данные профиля
    const { data: profile, refetch } = useQuery({
        queryKey: ['profile'],
        queryFn: getMyProfile,
        enabled: isOpen
    });

    // 2. Получаем список районов для выбора
    const { data: districts = [] } = useQuery<DistrictDto[]>({
        queryKey: ['districts'],
        queryFn: getDistricts,
        enabled: isOpen
    });

    const [editMode, setEditMode] = useState(false);
    const [avatarFile, setAvatarFile] = useState<File | undefined>();
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);

    const { register, handleSubmit, setValue, reset } = useForm<UpdateProfileDto>();

    // При открытии профиля или переключении в режим редактирования заполняем форму
    useEffect(() => {
        if (profile && editMode) {
            setValue('name', profile.name);
            setValue('homeAddress', profile.homeAddress);
            setValue('districtId', profile.district?.id);
        }
    }, [profile, editMode, setValue]);

    const mutation = useMutation({
        mutationFn: updateProfile,
        onSuccess: () => {
            setEditMode(false);
            setAvatarFile(undefined);
            setPreviewUrl(null);
            queryClient.invalidateQueries({ queryKey: ['profile'] });
            // Если изменилось имя или аватар, стоит обновить и данные в authStore
            refetch();
        },
    });

    const handleLogout = async () => {
        await logout();
        clearUser();
        onClose();
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            setAvatarFile(file);
            setPreviewUrl(URL.createObjectURL(file));
        }
    };

    const onSubmit = (data: UpdateProfileDto) => {
        // Добавляем файл в объект запроса перед отправкой
        mutation.mutate({
            ...data,
            avatarFile: avatarFile
        });
    };

    if (!user) return null;

    // Подготовка объекта пользователя для аватара без @ts-ignore
    const avatarData = {
        name: profile?.name || user.name,
        avatarUrl: previewUrl || profile?.avatarUrl || user.avatarUrl
    };

    return (
        <Modal isOpen={isOpen} onClose={() => { setEditMode(false); onClose(); }}>
            <div className={styles.profile}>
                <UserAvatar user={avatarData} size="large" />

                {editMode ? (
                    <form onSubmit={handleSubmit(onSubmit)} className={styles.formStack}>
                        <Input
                            placeholder="Имя"
                            {...register('name')}
                        />

                        <Input
                            placeholder="Домашний адрес"
                            {...register('homeAddress')}
                        />

                        <Select {...register('districtId')}>
                            <option value="">Выберите район</option>
                            {districts.map((d) => (
                                <option key={d.id} value={d.id}>{d.name}</option>
                            ))}
                        </Select>

                        <div className={styles.fileInputWrapper}>
                            <label className={styles.fileLabel}>
                                📸 Сменить фото профиля
                                <input
                                    type="file"
                                    accept="image/*"
                                    onChange={handleFileChange}
                                    hidden
                                />
                            </label>
                        </div>

                        <div className={styles.buttons}>
                            <Button type="submit" disabled={mutation.isPending}>
                                {mutation.isPending ? 'Сохранение...' : 'Сохранить'}
                            </Button>
                            <Button
                                variant="secondary"
                                onClick={() => {
                                    setEditMode(false);
                                    setPreviewUrl(null);
                                }}
                            >
                                Отмена
                            </Button>
                        </div>
                    </form>
                ) : (
                    <>
                        <h2 className={styles.name}>{profile?.name || user.name}</h2>
                        <div className={styles.infoBlock}>
                            <p className={styles.infoRow}><strong>Email:</strong> {profile?.email || user.email}</p>
                            <p className={styles.infoRow}><strong>Баллы:</strong> {profile?.points ?? user.points}</p>
                            <p className={styles.infoRow}>
                                <strong>Район:</strong> {profile?.district?.name || user.districtName || 'Не указан'}
                            </p>
                            {profile?.homeAddress && (
                                <p className={styles.infoRow}><strong>Адрес:</strong> {profile.homeAddress}</p>
                            )}
                        </div>

                        <div className={styles.buttons}>
                            <Button onClick={() => setEditMode(true)}>Редактировать профиль</Button>
                            <Button variant="outline" onClick={handleLogout}>Выйти</Button>
                        </div>
                    </>
                )}
            </div>
        </Modal>
    );
};