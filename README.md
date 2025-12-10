БД на DBeaver PostgreSQL
1. Создание таблицы событий
CREATE TABLE IF NOT EXISTS events (
    id SERIAL PRIMARY KEY,
    event_date DATE NOT NULL,
    category TEXT NOT NULL,
    ivent TEXT NOT NULL
);

3. Индекс для быстрого поиска по дате
CREATE INDEX IF NOT EXISTS idx_events_date ON events(event_date);

4. Хранимая процедура: получить все события за ноябрь 2025
CREATE OR REPLACE FUNCTION get_november_2025_events()
RETURNS TABLE (
    id INT,
    event_date DATE,
    category TEXT,
    ivent TEXT
)
LANGUAGE sql
AS $$
    SELECT e.id, e.event_date, e.category, e.ivent
    FROM events e
    WHERE e.event_date >= '2025-11-01'
      AND e.event_date <= '2025-11-30'
    ORDER BY e.event_date, e.id;
$$;

5. Хранимая процедура: добавить событие 
CREATE OR REPLACE FUNCTION add_event(
    p_event_date DATE,
    p_category TEXT,
    p_ivent TEXT
)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    new_id INT;
BEGIN
    IF p_event_date < '2025-11-01' OR p_event_date > '2025-11-30' THEN
        RAISE EXCEPTION 'Дата должна быть в ноябре 2025 года';
    END IF;

    INSERT INTO events (event_date, category, ivent)
    VALUES (p_event_date, p_category, p_ivent)
    RETURNING id INTO new_id;

    RETURN new_id;
END;
$$;

6. Хранимая процедура: обновить событие
CREATE OR REPLACE FUNCTION update_event(
    p_id INT,
    p_event_date DATE,
    p_category TEXT,
    p_ivent TEXT
)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
BEGIN
    IF p_event_date < '2025-11-01' OR p_event_date > '2025-11-30' THEN
        RAISE EXCEPTION 'Дата должна быть в ноябре 2025 года';
    END IF;

    UPDATE events
    SET event_date = p_event_date,
        category = p_category,
        ivent = p_ivent
    WHERE id = p_id;

    RETURN FOUND; -- TRUE, если строка обновлена
END;
$$;

7. Хранимая процедура: удалить событие
CREATE OR REPLACE FUNCTION delete_event(p_id INT)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM events WHERE id = p_id;
    RETURN FOUND;
END;
$$;

8. Тестовые данные
INSERT INTO events (event_date, category, ivent) VALUES
('2025-11-01', 'Выходной', 'Суббота — отдых'),
('2025-11-05', 'Встреча', 'Встреча команды разработчиков в 15:00 (Zoom)'),
('2025-11-12', 'Дедлайн', 'Сдача проекта: код, документация, тесты'),
('2025-11-17', 'Напоминание', 'Проверить лабораторную работу'),
('2025-11-20', 'Событие', 'День открытых дверей'),
('2025-11-25', 'Важное', 'Презентация проекта перед комиссией'),
('2025-11-30', 'Итоги', 'Подведение итогов месяца');
