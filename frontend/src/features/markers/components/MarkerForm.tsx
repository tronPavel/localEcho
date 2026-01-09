import { useForm } from 'react-hook-form';
import {Modal} from "../../../utils/modal/Modal.tsx";
import {useUIStore} from "../../../store"
import {useCreateMarker} from "../api/Markers.query.ts";
interface FormData {
    title: string;
    description?: string;
    category: 'Issue' | 'Event' | 'Announcement';
}

export const CreateMarkerForm = () => {
    const { pendingMarker, closeCreateMarkerModal } = useUIStore();
    const { mutate, isPending } = useCreateMarker();

    const {
        register,
        handleSubmit,
        formState: { errors },
        reset,
    } = useForm<FormData>({
        defaultValues: {
            category: 'Issue',
        },
    });

    const onSubmit = (data: FormData) => {
        if (!pendingMarker) return;

        mutate(
            {
                title: data.title,
                latitude: pendingMarker.lat,
                longitude: pendingMarker.lng,
                description: data.description || undefined,
                category: data.category,
            },
            {
                onSuccess: () => {
                    closeCreateMarkerModal();
                    reset();
                },
            }
        );
    };

    return (
        <Modal isOpen={true} onClose={closeCreateMarkerModal}>
            <h2 className="modal-title">Добавить метку</h2>

            <form onSubmit={handleSubmit(onSubmit)}>
                <div className="form-group">
                    <label>Заголовок *</label>
                    <input
                        {...register('title', { required: 'Обязательное поле' })}
                        placeholder="Например: Яма на дороге"
                    />
                    {errors.title && <span className="error">{errors.title.message}</span>}
                </div>

                <div className="form-group">
                    <label>Описание</label>
                    <textarea
                        {...register('description')}
                        rows={4}
                        placeholder="Подробности..."
                    />
                </div>

                <div className="form-group">
                    <label>Категория *</label>
                    <select {...register('category')}>
                        <option value="Issue">Проблема (яма, мусор и т.д.)</option>
                        <option value="Event">Мероприятие</option>
                        <option value="Announcement">Объявление</option>
                    </select>
                </div>

                <div className="form-actions">
                    <button type="button" onClick={closeCreateMarkerModal} disabled={isPending}>
                        Отмена
                    </button>
                    <button type="submit" disabled={isPending}>
                        {isPending ? 'Создаём...' : 'Создать метку'}
                    </button>
                </div>
            </form>
        </Modal>
    );
};