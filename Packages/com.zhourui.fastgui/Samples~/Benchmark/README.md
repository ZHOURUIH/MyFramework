# FastGUI 1.4.0 Publication Benchmark Sample

The sample mirrors the final release benchmark used to compare FastGUI with UGUI.

It includes the 2400-item inventory suite, 31 CPU cases, 13 render cases, prefab comparison, clone comparison, FastImage mutation coverage and 34 weakness/crossover cases.

The default benchmark profile is `Publication`. Historical optimization A/B switches, Runtime stage breakdowns and Direct Index diagnostic backends are intentionally excluded.

For published GPU claims, use GPU frame timing only when Unity reports valid device/driver timing. The reference Mali-G72 run did not expose valid GPU timing, so the 1.4.0 documentation makes no claim that FastGUI GPU execution time is lower than UGUI.
