import { useState, useEffect } from 'react';
import { toast } from 'sonner';
import { getImageUrl } from '@/shared/api/apiInstance';
import styles from './ImageUploader.module.css';

interface ImageUploaderProps {
    onFilesChange: (files: File[]) => void;
    maxFiles?: number;
    multiple?: boolean;
    label?: string;
    initialPreview?: string | null;
}

export const ImageUploader = ({
                                  onFilesChange,
                                  maxFiles = 10,
                                  multiple = true,
                                  label,
                                  initialPreview
                              }: ImageUploaderProps) => {
    const [files, setFiles] = useState<File[]>([]);

    const [previews, setPreviews] = useState<string[]>(
        initialPreview ? [getImageUrl(initialPreview)!] : []
    );

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (!e.target.files?.length) return;
        const newFiles = Array.from(e.target.files);

        if (multiple) {
            if (files.length + newFiles.length > maxFiles) {
                return toast.warning(`Максимально ${maxFiles} фото`);
            }
            const updatedFiles = [...files, ...newFiles];
            setFiles(updatedFiles);
            setPreviews(prev => [...prev, ...newFiles.map(f => URL.createObjectURL(f))]);
            onFilesChange(updatedFiles);
        } else {
            const file = newFiles[0];

            previews.forEach(p => {
                if (p.startsWith('blob:')) URL.revokeObjectURL(p);
            });

            const newBlob = URL.createObjectURL(file);
            setFiles([file]);
            setPreviews([newBlob]);
            onFilesChange([file]);
        }
    };

    const removeFile = (index: number) => {
        if (previews[index].startsWith('blob:')) {
            URL.revokeObjectURL(previews[index]);
        }

        const f = files.filter((_, i) => i !== index);
        const p = previews.filter((_, i) => i !== index);

        setFiles(f);
        setPreviews(p);
        onFilesChange(f);
    };

    useEffect(() => {
        return () => previews.forEach(p => {
            if (p.startsWith('blob:')) URL.revokeObjectURL(p);
        });
    }, []);

    return (
        <div className={styles.container}>
            {label && <label className={styles.label}>{label}</label>}

            <div className={styles.uploadCard}>
                <input
                    type="file"
                    multiple={multiple}
                    accept="image/*"
                    onChange={handleFileChange}
                    id="uploader"
                    hidden
                />
                <label htmlFor="uploader" className={styles.dropArea}>
                    <span className={styles.plus}>+</span>
                    <p>{multiple ? 'Добавить фотографии' : 'Заменить изображение'}</p>
                </label>
            </div>

            <div className={styles.grid}>
                {previews.map((url, i) => (
                    <div key={url} className={styles.previewItem}>
                        <img src={url} alt={`Превью ${i}`} />
                        <button
                            type="button"
                            className={styles.removeBtn}
                            onClick={(e) => {
                                e.preventDefault();
                                removeFile(i);
                            }}
                        >
                            ×
                        </button>
                    </div>
                ))}
            </div>
        </div>
    );
};