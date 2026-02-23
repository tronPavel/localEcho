import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useQuery, useMutation } from '@tanstack/react-query';
import { Modal } from '@/shared/ui/Modal/Modal';
import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { useAuthStore } from '../model/authStore';
import { logout } from '../model/authApi';
import { getMyProfile, updateProfile, uploadAvatar } from '@/entities/user/model/userApi';
import type { UpdateProfileDto } from '../model/types';
import styles from './ProfileModal.module.css';

export const ProfileModal = ({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) => {
    const { user, clearUser } = useAuthStore();

    const { data: profile, refetch } = useQuery({
        queryKey: ['profile'],
        queryFn: getMyProfile,
        enabled: isOpen
    });

    const [editMode, setEditMode] = useState(false);
    const { register, handleSubmit, setValue } = useForm<UpdateProfileDto>();
    const [avatarFile, setAvatarFile] = useState<File | undefined>();

    const mutation = useMutation({
        mutationFn: updateProfile,
        onSuccess: () => {
            setEditMode(false);
            refetch();
        },
    });

    const handleLogout = async () => {
        await logout();
        clearUser();
        onClose();
    };

    const onSubmit = async (data: UpdateProfileDto) => {
        if (avatarFile) {
            await uploadAvatar(avatarFile);
        }
        mutation.mutate(data);
    };

    if (!user) return null;

    if (profile && !editMode) {
        setValue('name', profile.name);
        setValue('homeAddress', profile.homeAddress);
    }

    const displayUser = profile ? {
        ...profile,
        roles: profile.roles
    } : user;

    return (
        <Modal isOpen={isOpen} onClose={onClose}>
            <div className={styles.profile}>
                {/* @ts-ignore*/}
                <UserAvatar user={displayUser} size="large" />

                {editMode ? (
                    <form onSubmit={handleSubmit(onSubmit)} className={styles.formStack}>
                        <Input placeholder="Имя" {...register('name')} />
                        <Input placeholder="Адрес" {...register('homeAddress')} />
                        <div className={styles.fileInputWrapper}>
                            <label>Сменить фото:</label>
                            <Input type="file" accept="image/*" onChange={(e) => setAvatarFile(e.target.files?.[0])} />
                        </div>
                        <div className={styles.buttons}>
                            <Button type="submit">Сохранить</Button>
                            <Button variant="secondary" onClick={() => setEditMode(false)}>Отмена</Button>
                        </div>
                    </form>
                ) : (
                    <>
                        <h2>{displayUser.name}</h2>
                        <p className={styles.infoRow}>Email: {displayUser.email}</p>
                        <p className={styles.infoRow}>Баллы: {displayUser.points}</p>
                        <p className={styles.infoRow}>
                            Район: {profile?.district?.name || user.districtName || 'Не указан'}
                        </p>
                        <div className={styles.buttons}>
                            <Button onClick={() => setEditMode(true)}>Редактировать</Button>
                            <Button variant="outline" onClick={handleLogout}>Выйти</Button>
                        </div>
                    </>
                )}
            </div>
        </Modal>
    );
};