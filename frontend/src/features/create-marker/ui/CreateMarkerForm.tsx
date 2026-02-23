import { useForm, type SubmitHandler } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { Textarea } from '@/shared/ui/Textarea/Textarea';
import { Select } from '@/shared/ui/Select/Select';
import { createMarkerSchema } from '../lib/validateMarker';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createMarker } from '../model/createMarkerApi';
import { useCreateMarkerStore } from '../model/createMarkerStore';
import { uploadFile } from '@/features/upload-file/model/uploadApi';
import { useState } from 'react';
import styles from './CreateMarkerForm.module.css';

interface FormData {
    title: string;
    description?: string;
    category: 'Issue' | 'Event' | 'Announcement';
}

export const CreateMarkerForm = () => {
    const { pendingPosition, closeModal } = useCreateMarkerStore();
    const queryClient = useQueryClient();

    const { register, handleSubmit, formState: { errors }, reset } = useForm<FormData>({
        resolver: zodResolver(createMarkerSchema),
        defaultValues: { category: 'Issue' },
    });

    const [selectedFile, setSelectedFile] = useState<File | null>(null);
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);

    const mutation = useMutation({
        mutationFn: async (data: FormData) => {
            console.log('🚀 Создание маркера начато', data);

            let imageUrl: string | undefined = undefined;
            if (selectedFile) {
                console.log('📤 Загружаем файл...', selectedFile.name);
                imageUrl = await uploadFile(selectedFile);
                console.log('✅ Файл загружен:', imageUrl);
            }

            await createMarker({
                title: data.title,
                description: data.description,
                category: data.category,
                latitude: pendingPosition!.lat,
                longitude: pendingPosition!.lng,
                imageUrl,
            });

            console.log('✅ Маркер успешно создан');
        },
        onSuccess: () => {
            console.log('🎉 onSuccess — обновляем список');
            queryClient.invalidateQueries({ queryKey: ['markers'] });
            closeModal();
            reset();
            setSelectedFile(null);
            setPreviewUrl(null);
            alert('Маркер успешно создан!');
        },
        onError: (err: any) => {
            console.error('❌ Ошибка создания маркера:', err);
            alert('Ошибка: ' + (err?.response?.data?.error || err.message || 'Неизвестная ошибка'));
        },
    });

    const onSubmit: SubmitHandler<FormData> = (data) => {
        console.log('📝 Форма отправлена');
        mutation.mutate(data);
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (file) {
            setSelectedFile(file);
            setPreviewUrl(URL.createObjectURL(file));
        }
    };

    if (!pendingPosition) return <div>Выберите точку на карте</div>;

    return (
        <form onSubmit={handleSubmit(onSubmit)} className={styles.form}>
            <h2>Добавить метку</h2>

            <Input placeholder="Заголовок *" {...register('title')} error={errors.title?.message} />
            <Textarea placeholder="Описание" {...register('description')} />

            <Select {...register('category')}>
                <option value="Issue">Проблема</option>
                <option value="Event">Мероприятие</option>
                <option value="Announcement">Объявление</option>
            </Select>

            <Input
                type="file"
                accept="image/*"
                onChange={handleFileChange}
            />

            {previewUrl && <img src={previewUrl} alt="Preview" className={styles.preview} />}

            <div className={styles.actions}>
                <Button variant="secondary" onClick={closeModal} disabled={mutation.isPending}>
                    Отмена
                </Button>
                <Button type="submit" disabled={mutation.isPending}>
                    {mutation.isPending ? 'Создаём...' : 'Создать метку'}
                </Button>
            </div>
        </form>
    );
};