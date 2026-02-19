# CS2_SmokeTeamColor
Плагин изменяет цвет дымовых гранат в зависимости от команды игрока, бросившего гранату. Для террористов и контр-террористов можно задать фиксированный цвет или включить случайный цвет при каждом броске. Поддерживаются боты.

# Требования
~~~
CounterStrikeSharp API версии 362 или выше
.NET 8.0 Runtime

~~~

# Конфигурационные параметры

~~~
css_smoketeamcolor_enabled <0/1>, def.=1 – Включение/выключение плагина.
css_smoketeamcolor_color_t (строка), def.="255 0 0" – Цвет дыма для террористов. Поддерживаются форматы: RGB (три числа через пробел, например "255 0 0"), HEX ("#FF0000"), название цвета ("red").
css_smoketeamcolor_color_ct (строка), def.="0 0 255" – Цвет дыма для контр-террористов. Поддерживаются те же форматы.
css_smoketeamcolor_random_t <0/1>, def.=0 – Использовать случайный цвет для террористов при каждом броске (1 – включено, 0 – используется фиксированный цвет из color_t).
css_smoketeamcolor_random_ct <0/1>, def.=0 – Использовать случайный цвет для контр-террористов при каждом броске.
css_smoketeamcolor_loglevel <0-5>, def.=4 – Уровень логирования (0-Trace,1-Debug,2-Info,3-Warning,4-Error,5-Critical).

~~~

# Консольные команды

~~~
css_smoketeamcolor_help – Показать подробную справку по плагину.
css_smoketeamcolor_settings – Показать текущие настройки плагина.
css_smoketeamcolor_test – Вывести в чат информацию о настройках для текущего игрока (доступно только игроку).
css_smoketeamcolor_reload – Перезагрузить конфигурацию из файла.
css_smoketeamcolor_setenabled <0/1> – Установить значение css_smoketeamcolor_enabled.
css_smoketeamcolor_setcolor_t <цвет> – Установить цвет для террористов (css_smoketeamcolor_color_t). Форматы: RGB ("255 0 0"), HEX ("#FF0000"), название ("red").
css_smoketeamcolor_setcolor_ct <цвет> – Установить цвет для контр-террористов (css_smoketeamcolor_color_ct).
css_smoketeamcolor_setrandom_t <0/1> – Установить значение css_smoketeamcolor_random_t.
css_smoketeamcolor_setrandom_ct <0/1> – Установить значение css_smoketeamcolor_random_ct.
css_smoketeamcolor_setloglevel <0-5> – Установить уровень логирования.

~~~
