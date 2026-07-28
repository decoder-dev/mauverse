from collections import deque
from hashlib import sha256
from math import ceil
from threading import Lock
from time import monotonic


class SlidingWindowRateLimiter:
    """Small process-local guard for abuse-prone upstream operations."""

    def __init__(self, requests: int, window_seconds: int, max_entries: int) -> None:
        self._requests = requests
        self._window_seconds = window_seconds
        self._max_entries = max_entries
        self._events: dict[bytes, deque[float]] = {}
        self._lock = Lock()

    @staticmethod
    def _digest(key: str) -> bytes:
        return sha256(key.encode("utf-8")).digest()

    def acquire(self, key: str) -> int | None:
        """Record an attempt or return the number of seconds until it may retry."""
        now = monotonic()
        cutoff = now - self._window_seconds
        digest = self._digest(key)

        with self._lock:
            events = self._events.get(digest)
            if events is None:
                self._evict_if_full(cutoff)
                events = deque()
                self._events[digest] = events

            while events and events[0] <= cutoff:
                events.popleft()

            if len(events) >= self._requests:
                return max(1, ceil(events[0] + self._window_seconds - now))

            events.append(now)
            return None

    def clear(self) -> None:
        with self._lock:
            self._events.clear()

    def _evict_if_full(self, cutoff: float) -> None:
        if len(self._events) < self._max_entries:
            return

        expired = [
            key for key, events in self._events.items() if not events or events[-1] <= cutoff
        ]
        for key in expired:
            self._events.pop(key, None)

        if len(self._events) >= self._max_entries:
            oldest_key = min(self._events, key=lambda key: self._events[key][-1])
            self._events.pop(oldest_key, None)
