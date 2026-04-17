import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { Textarea } from '@/shared/ui/Textarea/Textarea';
import { Select } from '@/shared/ui/Select/Select';
import { createMarkerSchema } from '../lib/validateMarker';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createMarker } from '../model/createMarkerApi';
import { useCreateMarkerStore } from '../model/createMarkerStore';
import styles from './CreateMarkerForm.module.css';

interface FormData {
    title: string;
    description?: string;
    category: 'Issue' | 'Event' | 'Announcement';
}

export const CreateMarkerForm = () => {
    const { pendingPosition, closeModal } = useCreateMarkerStore();
    const queryClient = useQueryClient();

    const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
    const [previewUrls, setPreviewUrls] = useState<string[]>([]);

    const { register, handleSubmit, formState: { errors }, reset } = useForm<FormData>({
        resolver: zodResolver(createMarkerSchema),
        defaultValues: { category: 'Issue' },
    });

    const mutation = useMutation({
        mutationFn: (data: FormData) => createMarker({
            ...data,
            latitude: pendingPosition!.lat,
            longitude: pendingPosition!.lng,
            imageFiles: selectedFiles // Передаем массив в API
        }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['markers'] });
            reset();
            setSelectedFiles([]);
            setPreviewUrls([]);
            closeModal();
        },
    });

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const files = Array.from(e.target.files);
            setSelectedFiles(prev => [...prev, ...files]);

            const newPreviews = files.map(file => URL.createObjectURL(file));
            setPreviewUrls(prev => [...prev, ...newPreviews]);
        }
    };

    const removeFile = (index: number) => {
        setSelectedFiles(prev => prev.filter((_, i) => i !== index));
        setPreviewUrls(prev => prev.filter((_, i) => i !== index));
    };

    if (!pendingPosition) return <div>Выберите точку на карте</div>;

    return (
        <form onSubmit={handleSubmit((data) => mutation.mutate(data))} className={styles.form}>
            <h2>Добавить метку</h2>

            <Input
                placeholder="Заголовок *"
                {...register('title')}
                error={errors.title?.message}
            />

            <Textarea
                placeholder="Описание"
                {...register('description')}
            />

            <Select {...register('category')}>
                <option value="Issue">Проблема</option>
                <option value="Event">Мероприятие</option>
                <option value="Announcement">Объявление</option>
            </Select>

            <div className={styles.imageSection}>
                <label className={styles.fileLabel}>
                    Добавить фотографии
                    <input
                        type="file"
                        multiple
                        accept="image/*"
                        onChange={handleFileChange}
                        style={{ display: 'none' }}
                    />
                </label>

                {/* Сетка превью */}
                <div className={styles.previewsGrid}>
                    {previewUrls.map((url, i) => (
                        <div key={url} className={styles.previewItem}>
                            <img src={url} alt="preview" />
                            <button
                                type="button"
                                className={styles.removeBadge}
                                onClick={() => removeFile(i)}
                            >
                                ×
                            </button>
                        </div>
                    ))}
                </div>
            </div>

            <div className={styles.actions}>
                <Button
                    variant="secondary"
                    type="button"
                    onClick={closeModal}
                    disabled={mutation.isPending}
                >
                    Отмена
                </Button>
                <Button
                    type="submit"
                    disabled={mutation.isPending}
                >
                    {mutation.isPending ? 'Загрузка...' : 'Создать метку'}
                </Button>
            </div>
        </form>
    );
};